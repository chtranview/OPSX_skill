namespace MyWeb.Models;

public class AppSettings
{
    public string TpiUbike { get; set; } = string.Empty;
}

public class DataCollectionSettings
{
    public int IntervalSeconds { get; set; } = 60;
    public int DataRetentionDays { get; set; } = 7;
}

