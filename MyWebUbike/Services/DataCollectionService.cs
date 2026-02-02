using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;

namespace MyWeb.Services;

public class DataCollectionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CollectionStateService _state;
    private readonly DataCollectionSettings _settings;
    private readonly ILogger<DataCollectionService> _logger;

    public DataCollectionService(IServiceProvider serviceProvider, CollectionStateService state, DataCollectionSettings settings, ILogger<DataCollectionService> logger)
    {
        _serviceProvider = serviceProvider;
        _state = state;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_settings.IntervalSeconds);
        _logger.LogInformation("資料收集服務已啟動，收集間隔: {IntervalSeconds} 秒", _settings.IntervalSeconds);
        
        using var timer = new PeriodicTimer(interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCollectionAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful stop
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "資料收集背景服務執行失敗");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // stopping
            }
        }
    }

    private async Task RunCollectionAsync(CancellationToken cancellationToken)
    {
        var status = _state.GetStatus();

        if (!status.isRunning || status.sessionId == Guid.Empty)
        {
            _logger.LogDebug("收集未執行：isRunning={IsRunning}, sessionId={SessionId}", status.isRunning, status.sessionId);
            return;
        }

        _logger.LogInformation("開始執行資料收集，Session: {SessionId}", status.sessionId);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var ubikeService = scope.ServiceProvider.GetRequiredService<IUbikeService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TpiUbikeDbContext>();

        var collectedAt = DateTime.UtcNow;
        
        try
        {
            var stations = await ubikeService.GetAllStationsAsync();
            _logger.LogInformation("成功取得 YouBike API 資料，站點數: {Count}", stations.Count);

            if (stations.Count == 0)
            {
                _logger.LogWarning("未取得任何 YouBike 站點資料，本次收集略過。Session: {SessionId}", status.sessionId);
                return;
            }

            var records = stations.Select(s => new TpiUbikeAreaRecord
        {
            SessionId = status.sessionId,
            CollectedTime = collectedAt,
            Sno = s.Sno,
            Sna = s.Sna,
            Snaen = s.Snaen,
            Sarea = s.Sarea,
            Sareaen = s.Sareaen,
            Ar = s.Ar,
            Aren = s.Aren,
            Quantity = s.Quantity,
            AvailableRentBikes = s.AvailableRentBikes,
            AvailableReturnBikes = s.AvailableReturnBikes,
            Act = s.Act,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            Mday = s.Mday,
            SrcUpdateTime = s.SrcUpdateTime,
            UpdateTime = s.UpdateTime,
            InfoTime = s.InfoTime,
            InfoDate = s.InfoDate
            }).ToList();

            await dbContext.TpiUbikeAreaRecords.AddRangeAsync(records, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            _state.AddRecordsCount(records.Count);

            _logger.LogInformation("完成 YouBike 資料收集，共 {Count} 筆，Session: {SessionId}", records.Count, status.sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "資料收集過程發生錯誤，Session: {SessionId}", status.sessionId);
            throw;
        }
    }
}
