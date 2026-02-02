using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Services;

namespace MyWeb.Controllers;

public class DataCollectionController : Controller
{
    private readonly CollectionStateService _stateService;
    private readonly TpiUbikeDbContext _dbContext;
    private readonly ILogger<DataCollectionController> _logger;

    public DataCollectionController(CollectionStateService stateService, TpiUbikeDbContext dbContext, ILogger<DataCollectionController> logger)
    {
        _stateService = stateService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var status = _stateService.GetStatus();
        ViewBag.IsRunning = status.isRunning;
        ViewBag.SessionId = status.sessionId;
        ViewBag.StartTime = status.startUtc;
        ViewBag.RecordCount = status.recordCount;
        ViewBag.SelectedAreas = status.areas;
        ViewBag.DurationSeconds = status.startUtc.HasValue
            ? (int)Math.Max(0, (DateTime.UtcNow - status.startUtc.Value).TotalSeconds)
            : 0;
        return View();
    }

    [HttpPost]
    public IActionResult Start(List<string>? areas)
    {
        try
        {
            _stateService.Start(areas);
            TempData["Message"] = "資料收集已啟動";
            _logger.LogInformation("資料收集已啟動，選定行政區：{Areas}", string.Join(", ", _stateService.SelectedAreas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "啟動資料收集失敗");
            TempData["Error"] = $"啟動失敗：{ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // 便於 CLI 測試：提供 GET 別名
    [HttpGet("Start")]
    public IActionResult StartGet([FromQuery] List<string>? areas)
    {
        return Start(areas);
    }

    // 便於 CLI 測試：提供 JSON 版啟動（GET/POST 皆可）
    [HttpPost("startJson")]
    public IActionResult StartJson([FromForm] List<string>? areas)
    {
        try
        {
            _stateService.Start(areas);
            var status = _stateService.GetStatus();
            return Ok(new
            {
                ok = true,
                sessionId = status.sessionId,
                sessionStartTimeUtc = status.startUtc,
                selectedAreas = status.areas
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StartJson 失敗");
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    [HttpGet("startJson")]
    public IActionResult StartJsonGet([FromQuery] List<string>? areas) => StartJson(areas);

    [HttpPost]
    public IActionResult Stop()
    {
        try
        {
            var status = _stateService.GetStatus();
            _stateService.Stop();
            TempData["Message"] = $"資料收集已停止，共收集 {status.recordCount} 筆";
            _logger.LogInformation("資料收集已停止，Session: {SessionId}, 總筆數: {Count}", status.sessionId, status.recordCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止資料收集失敗");
            TempData["Error"] = $"停止失敗：{ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // 便於 CLI 測試：提供 GET 別名
    [HttpGet("Stop")]
    public IActionResult StopGet()
    {
        return Stop();
    }

    // 便於 CLI 測試：提供 JSON 版停止（GET/POST 皆可）
    [HttpPost("stopJson")]
    public IActionResult StopJson()
    {
        try
        {
            var status = _stateService.GetStatus();
            _stateService.Stop();
            return Ok(new
            {
                ok = true,
                sessionId = status.sessionId,
                recordCount = status.recordCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StopJson 失敗");
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    [HttpGet("stopJson")]
    public IActionResult StopJsonGet() => StopJson();

    [HttpGet]
    public IActionResult GetStatus()
    {
        var status = _stateService.GetStatus();
        var localStartTime = status.startUtc?.ToLocalTime();

        var distinctAreas = Array.Empty<string>();

        if (status.sessionId != Guid.Empty)
        {
            try
            {
                distinctAreas = _dbContext.TpiUbikeAreaRecords
                    .Where(x => x.SessionId == status.sessionId)
                    .Select(x => x.Sarea)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "查詢行政區列表失敗");
            }
        }

        return Json(new
        {
            isRunning = status.isRunning,
            sessionId = status.sessionId.ToString(),
            sessionStartTime = localStartTime?.ToString("yyyy-MM-dd HH:mm:ss"),
            recordCount = status.recordCount,
            durationSeconds = status.startUtc.HasValue
                ? (int)Math.Max(0, (DateTime.UtcNow - status.startUtc.Value).TotalSeconds)
                : 0,
            selectedAreas = status.areas.ToArray(),
            distinctAreas
        });
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(List<string>? areas)
    {
        try
        {
            var status = _stateService.GetStatus();

            if (status.sessionId == Guid.Empty)
            {
                TempData["Error"] = "無可匯出的資料（Session 未啟動）";
                return RedirectToAction(nameof(Index));
            }

            var query = _dbContext.TpiUbikeAreaRecords
                .Where(x => x.SessionId == status.sessionId)
                .AsQueryable();

            if (areas is { Count: > 0 })
            {
                query = query.Where(x => areas.Contains(x.Sarea));
            }

            // 只查詢需要的列以優化效能
            var records = await query
                .OrderBy(x => x.CollectedTime)
                .ThenBy(x => x.Sarea)
                .ThenBy(x => x.Sno)
                .Select(x => new
                {
                    x.CollectedTime,
                    x.Sno,
                    x.Sna,
                    x.Sarea,
                    x.AvailableRentBikes,
                    x.AvailableReturnBikes,
                    x.Quantity,
                    x.Act,
                    x.Ar,
                    x.Latitude,
                    x.Longitude
                })
                .ToListAsync();

            if (records.Count == 0)
            {
                TempData["Error"] = "無符合條件的資料可匯出";
                return RedirectToAction(nameof(Index));
            }

            var sb = new StringBuilder();
            sb.AppendLine("收集時間,站點編號,站點名稱,行政區,可借車輛,可還空位,總停車格,營運狀態,地址,緯度,經度");

            foreach (var record in records)
            {
                var localTime = record.CollectedTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                var actText = record.Act == "1" ? "營運中" : "暫停";

                sb.AppendLine($"{localTime},{Escape(record.Sno)},{Escape(record.Sna)},{Escape(record.Sarea)},{record.AvailableRentBikes},{record.AvailableReturnBikes},{record.Quantity},{actText},{Escape(record.Ar)},{record.Latitude},{record.Longitude}");
            }

            // 使用 UTF-8 BOM 確保 Excel/記事本顯示中文正常
            var utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            var bytes = utf8Bom.GetBytes(sb.ToString());
            var session8 = status.sessionId.ToString("N").Substring(0, 8);
            var fileName = $"ubike_session_{session8}_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯出 CSV 失敗");
            TempData["Error"] = $"匯出失敗：{ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
