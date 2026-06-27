using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using ForgeUI.Models;

namespace ForgeUI.Services;

public class HypixelApiService : IDisposable
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public HypixelApiService(AppConfig config)
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("API-Key", config.HypixelApi.ApiKey);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _baseUrl = config.HypixelApi.BaseUrl;
    }

    public async Task<List<SkyblockProfile>?> GetProfilesAsync(string playerUuid)
    {
        var url = $"{_baseUrl}/skyblock/profiles?uuid={playerUuid}";
        var response = await _http.GetStringAsync(url);
        var data = JsonConvert.DeserializeObject<ProfilesResponse>(response);
        return data?.Success == true ? data.Profiles : null;
    }

    public async Task<SkyblockProfileFull?> GetFullProfileAsync(string profileId)
    {
        var url = $"{_baseUrl}/skyblock/profile?profile={profileId}";
        var response = await _http.GetStringAsync(url);
        var data = JsonConvert.DeserializeObject<ProfileResponse>(response);
        return data?.Success == true ? data.Profile : null;
    }

public async Task<string?> GetUuidFromUsernameAsync(string username)
{
    try
    {
        var url = $"https://api.mojang.com/users/profiles/minecraft/{username}";
        var json = await _http.GetStringAsync(url);
        var doc = System.Text.Json.JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null;
    }
    catch { return null; }
}

    public async Task<Dictionary<string, string>> GetUsernamesAsync(IEnumerable<string> uuids)
    {
        var result = new Dictionary<string, string>();
        foreach (var uuid in uuids)
        {
            try
            {
                var url = $"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}";
                var json = await _http.GetStringAsync(url);
                var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("name", out var name))
                    result[uuid] = name.GetString() ?? uuid;
            }
            catch { result[uuid] = uuid; }
        }
        return result;
    }

    public async Task<Dictionary<string, BazaarItem>> GetBazaarAsync()
    {
        var url = $"{_baseUrl}/skyblock/bazaar";
        var response = await _http.GetStringAsync(url);
        var data = JsonConvert.DeserializeObject<BazaarResponse>(response);
        return data?.Success == true ? data.Products : new();
    }

    public void Dispose() => _http.Dispose();
}