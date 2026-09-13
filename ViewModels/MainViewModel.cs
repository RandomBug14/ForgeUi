using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using ForgeUI.Models;
using ForgeUI.Services;
using System.Net.Http;

namespace ForgeUI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly HypixelApiService _api;

    public event Action? OnDataLoaded;
    private readonly AppConfig _config;
    private readonly DispatcherTimer _refreshTimer;
    private readonly DispatcherTimer _tickTimer;
    public event Action? OnMembersRefreshed;
    public BazaarViewModel Bazaar { get; }
    public ICommand? RefreshCommand { get; set; }
    public ObservableCollection<MemberForgeViewModel> Members { get; } = new();

    private string _playerUuidClean = string.Empty;
    private string _cachedProfileId = string.Empty;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); }
    }

    private string _profileName = "—";
    public string ProfileName
    {
        get => _profileName;
        set { _profileName = value; OnPropertyChanged(); }
    }

    private string _statusMessage = "Entrez un pseudo pour commencer";
    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanRefresh)); }
    }

    public bool CanRefresh => !IsLoading;

    private DateTime? _lastUpdate;
    public DateTime? LastUpdate
    {
        get => _lastUpdate;
        set { _lastUpdate = value; OnPropertyChanged(); OnPropertyChanged(nameof(LastUpdateDisplay)); }
    }

    public string LastUpdateDisplay => LastUpdate.HasValue
        ? $"Dernière mise à jour : {LastUpdate.Value:HH:mm:ss}"
        : "Jamais mis à jour";

    private int _nextRefreshIn;
    public int NextRefreshIn
    {
        get => _nextRefreshIn;
        set { _nextRefreshIn = value; OnPropertyChanged(); OnPropertyChanged(nameof(NextRefreshDisplay)); }
    }

    public string NextRefreshDisplay => $"Prochain refresh dans {NextRefreshIn}s";
    public string AutoRefreshDisplay => $"{_config.App.AutoRefreshSeconds}s";

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasMembers => Members.Count > 0;

    public MainViewModel(HypixelApiService api, AppConfig config)
{
    _api = api;
    _config = config;
    Bazaar = new BazaarViewModel(api);

    // UUID par défaut depuis la config
    _playerUuidClean = config.Player.UUID.Replace("-", "");

    _refreshTimer = new DispatcherTimer
    {
        Interval = TimeSpan.FromSeconds(config.App.AutoRefreshSeconds)
    };
    _refreshTimer.Tick += async (_, _) =>
    {
        NextRefreshIn = config.App.AutoRefreshSeconds;
        await LoadDataAsync();
    };

    _tickTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
    _tickTimer.Tick += (_, _) =>
    {
        NextRefreshIn = Math.Max(0, NextRefreshIn - 1);
        foreach (var member in Members)
        {
            foreach (var slot in member.Slots)
                slot.Refresh();
            member.RefreshCounts();
        }
        OnMembersRefreshed?.Invoke();
    };

    Members.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasMembers));
}

    // ── Recherche par pseudo ──────────────────────────────────────────────────

    public async Task SearchPlayerAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return;
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = $"Recherche de « {username} »…";

        try
        {
            // Mojang : pseudo → UUID
            var uuid = await _api.GetUuidFromUsernameAsync(username);
            if (uuid == null)
            {
                System.Windows.MessageBox.Show(
                    $"Joueur « {username} » introuvable.",
                    "Joueur introuvable",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                StatusMessage = "Joueur introuvable";
                return;
            }

            _playerUuidClean = uuid;

            // Requête 1 : liste des profils
            var profiles = await _api.GetProfilesAsync(_playerUuidClean);
            if (profiles == null || profiles.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    $"Aucun profil Skyblock trouvé pour « {username} ».",
                    "Aucun profil",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                StatusMessage = "Aucun profil trouvé";
                return;
            }

            var target = profiles.FirstOrDefault(p =>
                              p.CuteName.Equals(_config.App.TargetProfileName, StringComparison.OrdinalIgnoreCase))
                          ?? profiles.FirstOrDefault(p => p.Selected)
                          ?? profiles.First();

            _cachedProfileId = target.ProfileId;
            ProfileName = target.CuteName;

            // Charge les membres
            await LoadMembersAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Erreur : {ex.Message}",
                "Erreur",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            StatusMessage = "Échec de la recherche";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Refresh auto (utilise le profil déjà connu) ───────────────────────────

    public async Task LoadDataAsync()
    {
        if (string.IsNullOrEmpty(_cachedProfileId)) return;
        await LoadMembersAsync();
    }

    // ── Chargement des membres (requête 2 seulement) ──────────────────────────

    private async Task LoadMembersAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Chargement des membres…";

        try
        {
            var fullProfile = await _api.GetFullProfileAsync(_cachedProfileId);
            if (fullProfile == null)
            {
                ErrorMessage = "Impossible de charger le profil complet.";
                return;
            }

            Members.Clear();

            foreach (var (uuid, member) in fullProfile.Members)
            {
                if (_config.App.ExcludedUUIDs.Any(e => e.Replace("-", "") == uuid.Replace("-", ""))) continue;

                var isMe = uuid.Replace("-", "") == _playerUuidClean;
                var vm = new MemberForgeViewModel
                {
                    MemberUuid = uuid,
                    IsCurrentPlayer = isMe,
                    DisplayName = isMe ? "👑 Vous" : $"Membre {uuid[..8]}…"
                };

                if (member.Forge?.ForgeProcesses != null)
                {
                    foreach (var (_, forgeGroup) in member.Forge.ForgeProcesses)
                    {
                        foreach (var (_, slot) in forgeGroup)
                        {
                            var itemInfo = ForgeItemDatabase.Get(slot.Id);
                            var startUtc = DateTimeOffset.FromUnixTimeMilliseconds(slot.StartTime).UtcDateTime;
                            DateTime? endUtc = itemInfo.Duration > TimeSpan.Zero
                                ? startUtc + itemInfo.Duration
                                : null;

                            var slotVm = new ForgeSlotViewModel
                            {
                                MemberUuid = uuid,
                                SlotNumber = slot.Slot,
                                ItemId = slot.Id,
                                ItemDisplayName = itemInfo.DisplayName,
                                ForgeType = slot.Type,
                                StartTime = startUtc,
                                EndTime = endUtc,
                            };
                            slotVm.Refresh();
                            vm.Slots.Add(slotVm);
                        }
                    }
                }

                vm.RefreshCounts();
                Members.Add(vm);
            }

            // Pseudos Mojang
            var uuids = fullProfile.Members.Keys.ToList();
            var usernames = await _api.GetUsernamesAsync(uuids);
            foreach (var vm in Members)
            {
                var isMe = vm.MemberUuid.Replace("-", "") == _playerUuidClean;
                var pseudo = usernames.TryGetValue(vm.MemberUuid, out var name) ? name : vm.MemberUuid[..8];
                vm.DisplayName = isMe ? $"👑 {pseudo}" : pseudo;
            }

            StatusMessage = "Chargement du Bazaar…";
            await Bazaar.RefreshAsync();
OnDataLoaded?.Invoke();
            LastUpdate = DateTime.Now;
            NextRefreshIn = _config.App.AutoRefreshSeconds;
            StatusMessage = $"✔ {Members.Count} membre(s) chargé(s)";
        }
        catch (HttpRequestException httpEx)
        {
            ErrorMessage = $"Erreur réseau : {httpEx.Message}";
            StatusMessage = "Échec du chargement";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur : {ex.Message}";
            StatusMessage = "Échec du chargement";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void StartTimers()
    {
        _refreshTimer.Start();
        _tickTimer.Start();
        Bazaar.StartTimer();
        NextRefreshIn = _config.App.AutoRefreshSeconds;
    }

    public void StopTimers()
    {
        _refreshTimer.Stop();
        _tickTimer.Stop();
        Bazaar.StopTimer();
    }
public async Task LoadDefaultPlayerAsync()
{
    if (IsLoading) return;
    IsLoading = true;
    ErrorMessage = string.Empty;
    StatusMessage = "Chargement du profil par défaut…";

    try
    {
        var profiles = await _api.GetProfilesAsync(_config.Player.UUID);
        if (profiles == null || profiles.Count == 0)
        {
            ErrorMessage = "Aucun profil trouvé.";
            return;
        }

        var target = profiles.FirstOrDefault(p =>
                          p.CuteName.Equals(_config.App.TargetProfileName, StringComparison.OrdinalIgnoreCase))
                      ?? profiles.FirstOrDefault(p => p.Selected)
                      ?? profiles.First();

        _cachedProfileId = target.ProfileId;
        ProfileName = target.CuteName;

        await LoadMembersAsync();
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Erreur : {ex.Message}";
    }
    finally
    {
        IsLoading = false;
    }
}
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}