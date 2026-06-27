namespace ForgeUI.Models;

public class AppConfig
{
    public HypixelApiConfig HypixelApi { get; set; } = new();
    public PlayerConfig Player { get; set; } = new();
    public AppSettings App { get; set; } = new();
}

public class HypixelApiConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.hypixel.net/v2";
}

public class PlayerConfig
{
    public string UUID { get; set; } = string.Empty;
}

public class AppSettings
{
    public int AutoRefreshSeconds { get; set; } = 60;
    public string TargetProfileName { get; set; } = "Blueberry";
    public List<string> ExcludedUUIDs { get; set; } = new();
}