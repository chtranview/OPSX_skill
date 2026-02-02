using System.Text.Json.Serialization;

namespace MyWeb.DTOs;

public class UbikeStationDto
{
    [JsonPropertyName("sno")]
    public string Sno { get; set; } = string.Empty;

    [JsonPropertyName("sna")]
    public string Sna { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("available_rent_bikes")]
    public int AvailableRentBikes { get; set; }

    [JsonPropertyName("sarea")]
    public string Sarea { get; set; } = string.Empty;

    [JsonPropertyName("mday")]
    public string Mday { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("ar")]
    public string Ar { get; set; } = string.Empty;

    [JsonPropertyName("sareaen")]
    public string Sareaen { get; set; } = string.Empty;

    [JsonPropertyName("snaen")]
    public string Snaen { get; set; } = string.Empty;

    [JsonPropertyName("aren")]
    public string Aren { get; set; } = string.Empty;

    [JsonPropertyName("available_return_bikes")]
    public int AvailableReturnBikes { get; set; }

    [JsonPropertyName("act")]
    public string Act { get; set; } = string.Empty;

    [JsonPropertyName("srcUpdateTime")]
    public string? SrcUpdateTime { get; set; }

    [JsonPropertyName("updateTime")]
    public string? UpdateTime { get; set; }

    [JsonPropertyName("infoTime")]
    public string? InfoTime { get; set; }

    [JsonPropertyName("infoDate")]
    public string? InfoDate { get; set; }
}

public class UbikeAreaQueryResponseDto
{
    public string Area { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<UbikeStationDto> Stations { get; set; } = new();
}

