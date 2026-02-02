using System.Text.Json;
using MyWeb.DTOs;
using MyWeb.Models;

namespace MyWeb.Services;

public interface IUbikeService
{
    Task<UbikeAreaQueryResponseDto> AreaQryAsync(string area);
    Task<List<UbikeStationDto>> GetAllStationsAsync();
}

public class UbikeService : IUbikeService
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _appSettings;
    private readonly ILogger<UbikeService> _logger;

    public UbikeService(HttpClient httpClient, AppSettings appSettings, ILogger<UbikeService> logger)
    {
        _httpClient = httpClient;
        _appSettings = appSettings;
        _logger = logger;
    }

    public async Task<UbikeAreaQueryResponseDto> AreaQryAsync(string area)
    {
        try
        {
            var stations = await FetchStationsAsync();

            var filteredStations = stations
                .Where(s => !string.IsNullOrEmpty(s.Sarea) &&
                           s.Sarea.Equals(area, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return new UbikeAreaQueryResponseDto
            {
                Area = area,
                Count = filteredStations.Count,
                Stations = filteredStations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询 YouBike 数据时发生错误，行政区域: {Area}", area);
            throw;
        }
    }

    public Task<List<UbikeStationDto>> GetAllStationsAsync() => FetchStationsAsync();

    private async Task<List<UbikeStationDto>> FetchStationsAsync()
    {
        try
        {
            var apiUrl = _appSettings.TpiUbike;

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new InvalidOperationException("tpiUbike 配置未设置");
            }

            using var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<UbikeStationDto>>(jsonContent, options)
                   ?? new List<UbikeStationDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "获取 YouBike 数据时发生 HTTP 错误");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "解析 YouBike JSON 数据时发生错误");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 YouBike 数据时发生未知错误");
            throw;
        }
    }
}

