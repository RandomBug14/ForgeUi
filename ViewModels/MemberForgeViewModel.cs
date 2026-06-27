using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ForgeUI.ViewModels;

public class MemberForgeViewModel : INotifyPropertyChanged
{
    public string MemberUuid { get; init; } = string.Empty;
    public bool IsCurrentPlayer { get; init; }

    private string _displayName = string.Empty;
    public string DisplayName
    {
        get => _displayName;
        set { _displayName = value; OnPropertyChanged(); }
    }


    public ObservableCollection<ForgeSlotViewModel> Slots { get; } = new();

    private int _doneCount;
    public int DoneCount
    {
        get => _doneCount;
        set { _doneCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(Summary)); }
    }

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        set { _totalCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(Summary)); }
    }

    public string Summary => $"{DoneCount}/{TotalCount} terminé(s)";

    public bool HasNoSlots => Slots.Count == 0;

    public void RefreshCounts()
    {
        DoneCount = Slots.Count(s => s.IsDone);
        TotalCount = Slots.Count;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
