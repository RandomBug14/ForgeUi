namespace ForgeUI.Models;

/// <summary>
/// Base de données locale des items de forge Skyblock.
/// Durées en millisecondes (sans réduction).
/// Source : wiki.hypixel.net/Forge
/// </summary>
public static class ForgeItemDatabase
{
    // Durées en ms
private static readonly Dictionary<string, ForgeItemInfo> Items = new(StringComparer.OrdinalIgnoreCase)
{
    { "BEJEWELED_HANDLE",   new("Bejeweled Handle",  TimeSpan.FromSeconds(21)) },
    { "MITHRIL_PLATE",      new("Mithril Plate",      new TimeSpan(12, 36, 0)) },
    { "TUNGSTEN_PLATE",     new("Tungsten Plate",     new TimeSpan(2, 6, 0))   },
    { "UMBER_PLATE",        new("Umber Plate",        new TimeSpan(2, 6, 0))   },
    { "PERFECT_PLATE",      new("Perfect Plate",      TimeSpan.FromMinutes(21))},
    { "SKELETON_KEY",       new("Skeleton Key",       TimeSpan.FromMinutes(21))},
};

    public static ForgeItemInfo Get(string id)
    {
        if (Items.TryGetValue(id, out var info))
            return info;

        // Item inconnu : on ne connaît pas la durée
        return new ForgeItemInfo(id, TimeSpan.Zero);
    }
}

public record ForgeItemInfo(string DisplayName, TimeSpan Duration);
