using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;

namespace MyWeb.Services;

public class DataCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DataCollectionSettings _settings;
    private readonly ILogger<DataCleanupService> _logger;

    public DataCleanupService(IServiceProvider serviceProvider, DataCollectionSettings settings, ILogger<DataCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromHours(1); // 每小時檢查一次
        _logger.LogInformation("資料清理服務已啟動，保留天數: {RetentionDays} 天", _settings.DataRetentionDays);
        
        using var timer = new PeriodicTimer(interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理舊資料時發生錯誤");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-_settings.DataRetentionDays);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TpiUbikeDbContext>();

        var deleted = await dbContext.TpiUbikeAreaRecords
            .Where(x => x.CollectedTime < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
        {
            _logger.LogInformation("已清理 {Deleted} 筆 7 天前資料", deleted);
        }
    }
}
