using System.IO;
using System.Text.Json;
using ForgeUI.Models;

namespace ForgeUI.Services;

public static class ConfigService
{
    public static AppConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new AppConfig();
    }
}