using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ForgeUI.ViewModels;

public class ForgeSlotViewModel : INotifyPropertyChanged
{
    // ── Données brutes ────────────────────────────────────────────────────────

    public string MemberUuid { get; init; } = string.Empty;
    public int SlotNumber { get; init; }
    public string ItemId { get; init; } = string.Empty;
    public string ItemDisplayName { get; init; } = string.Empty;
    public string ForgeType { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime? EndTime { get; init; }  // null = durée inconnue

    // ── Propriétés calculées (mises à jour chaque seconde) ────────────────────

    private string _remainingDisplay = string.Empty;
    public string RemainingDisplay
    {
        get => _remainingDisplay;
        set { _remainingDisplay = value; OnPropertyChanged(); }
    }

    private double _progressPercent;
    public double ProgressPercent
    {
        get => _progressPercent;
        set { _progressPercent = value; OnPropertyChanged(); }
    }

    private bool _isDone;
    public bool IsDone
    {
        get => _isDone;
        set { _isDone = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusColor)); }
    }

    private bool _isUnknownDuration;
    public bool IsUnknownDuration
    {
        get => _isUnknownDuration;
        set { _isUnknownDuration = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusColor)); }
    }

    public string StatusColor => IsDone ? "#4CAF50" : IsUnknownDuration ? "#FF9800" : "#2196F3";

    // ── Mise à jour ───────────────────────────────────────────────────────────

    public void Refresh()
    {
        var now = DateTime.UtcNow;

        if (EndTime == null)
        {
            IsUnknownDuration = true;
            IsDone = false;
            RemainingDisplay = "Durée inconnue";
            ProgressPercent = 0;
            return;
        }

        var end = EndTime.Value;
        var totalSpan = end - StartTime;
        var elapsed = now - StartTime;

        if (now >= end)
        {
            IsDone = true;
            IsUnknownDuration = false;
            RemainingDisplay = "✔ Terminé";
            ProgressPercent = 100;
        }
        else
        {
            IsDone = false;
            IsUnknownDuration = false;
            var remaining = end - now;
            RemainingDisplay = FormatRemaining(remaining);
            ProgressPercent = totalSpan.TotalSeconds > 0
                ? Math.Min(100, elapsed.TotalSeconds / totalSpan.TotalSeconds * 100)
                : 0;
        }
    }

    private static string FormatRemaining(TimeSpan t)
    {
        if (t.TotalHours >= 1)
            return $"{(int)t.TotalHours}h {t.Minutes:D2}m {t.Seconds:D2}s";
        if (t.TotalMinutes >= 1)
            return $"{t.Minutes}m {t.Seconds:D2}s";
        return $"{t.Seconds}s";
    }

    // ── INotifyPropertyChanged ────────────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
