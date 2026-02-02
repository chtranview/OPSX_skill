namespace MyWeb.Services;

public class CollectionStateService
{
    private readonly object _lock = new();
    private readonly ILogger<CollectionStateService> _logger;

    public bool IsRunning { get; private set; }
    public Guid CurrentSessionId { get; private set; }
    public DateTime? SessionStartTimeUtc { get; private set; }
    public int TotalRecordsInSession { get; private set; }
    public IReadOnlyList<string> SelectedAreas => _selectedAreas;

    private List<string> _selectedAreas = new();

    public CollectionStateService(ILogger<CollectionStateService> logger)
    {
        _logger = logger;
    }

    public void Start(IEnumerable<string>? areas = null)
    {
        lock (_lock)
        {
            IsRunning = true;
            CurrentSessionId = Guid.NewGuid();
            SessionStartTimeUtc = DateTime.UtcNow;
            TotalRecordsInSession = 0;
            _selectedAreas = NormalizeAreas(areas).ToList();
            _logger.LogInformation("收集已啟動，Session: {SessionId}, 篩選行政區數: {Count}", CurrentSessionId, _selectedAreas.Count);
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            _logger.LogInformation("收集已停止，Session: {SessionId}, 累計筆數: {Count}", CurrentSessionId, TotalRecordsInSession);
            IsRunning = false;
        }
    }

    public void AddRecordsCount(int count)
    {
        if (count == 0)
        {
            return;
        }

        lock (_lock)
        {
            TotalRecordsInSession += count;
        }
    }

    public (bool isRunning, Guid sessionId, DateTime? startUtc, int recordCount, IReadOnlyList<string> areas) GetStatus()
    {
        lock (_lock)
        {
            return (IsRunning, CurrentSessionId, SessionStartTimeUtc, TotalRecordsInSession, _selectedAreas);
        }
    }

    private static IEnumerable<string> NormalizeAreas(IEnumerable<string>? areas)
    {
        if (areas is null)
        {
            yield break;
        }

        foreach (var area in areas)
        {
            if (!string.IsNullOrWhiteSpace(area))
            {
                yield return area.Trim();
            }
        }
    }
}
