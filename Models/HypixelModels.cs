using Newtonsoft.Json;

namespace ForgeUI.Models;

// ── Profils ──────────────────────────────────────────────────────────────────

public class ProfilesResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("profiles")]
    public List<SkyblockProfile> Profiles { get; set; } = new();
}

public class SkyblockProfile
{
    [JsonProperty("profile_id")]
    public string ProfileId { get; set; } = string.Empty;

    [JsonProperty("cute_name")]
    public string CuteName { get; set; } = string.Empty;

    [JsonProperty("selected")]
    public bool Selected { get; set; }

    [JsonProperty("members")]
    public Dictionary<string, ProfileMember>? Members { get; set; }
}

// ── Profil complet ────────────────────────────────────────────────────────────

public class ProfileResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("profile")]
    public SkyblockProfileFull? Profile { get; set; }
}

public class SkyblockProfileFull
{
    [JsonProperty("profile_id")]
    public string ProfileId { get; set; } = string.Empty;

    [JsonProperty("cute_name")]
    public string CuteName { get; set; } = string.Empty;

    [JsonProperty("members")]
    public Dictionary<string, ProfileMember> Members { get; set; } = new();
}

// ── Membre & Forge ────────────────────────────────────────────────────────────

public class ProfileMember
{
    [JsonProperty("forge")]
    public ForgeData? Forge { get; set; }
}

public class ForgeData
{
    [JsonProperty("forge_processes")]
    public Dictionary<string, Dictionary<string, ForgeSlot>> ForgeProcesses { get; set; } = new();
}

public class ForgeSlot
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("startTime")]
    public long StartTime { get; set; }

    [JsonProperty("slot")]
    public int Slot { get; set; }

    [JsonProperty("notified")]
    public bool Notified { get; set; }

    [JsonProperty("oldItem")]
    public object? OldItem { get; set; }

    public class BazaarResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("products")]
        public Dictionary<string, BazaarItem> Products { get; set; } = new();
    }

    public class BazaarItem
    {
        [JsonProperty("quick_status")]
        public BazaarQuickStatus QuickStatus { get; set; } = new();
    }

    public class BazaarQuickStatus
    {
        [JsonProperty("buyPrice")]
        public double BuyPrice { get; set; }

        [JsonProperty("sellPrice")]
        public double SellPrice { get; set; }
    }


}

public class BazaarResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("products")]
    public Dictionary<string, BazaarItem> Products { get; set; } = new();
}

public class BazaarItem
{
    [JsonProperty("quick_status")]
    public BazaarQuickStatus QuickStatus { get; set; } = new();
}

public class BazaarQuickStatus
{
    [JsonProperty("buyPrice")]
    public double BuyPrice { get; set; }

    [JsonProperty("sellPrice")]
    public double SellPrice { get; set; }
}
