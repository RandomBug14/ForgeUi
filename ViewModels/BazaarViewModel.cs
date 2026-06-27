using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using ForgeUI.Models;
using ForgeUI.Services;

namespace ForgeUI.ViewModels;

public class BazaarViewModel : INotifyPropertyChanged
{
    private readonly HypixelApiService _api;
    private readonly DispatcherTimer _refreshTimer;

    private double _mithrilPlatePrice;
    private double _glaciteAmalgamationPrice;
    private double _refinedTungstenPrice;
    private double _refinedUmberPrice;
    private double _glaciteJewelPrice;
    private double _skeletonKeySellPrice;

    private const int SlotsPerPerson = 7;
    private const int MembersCount = 5;

    // 2h06 + 2h06 + 21min + 21min = 4h54
    private const double CycleHours = 4.0 + 54.0 / 60.0;

    public BazaarViewModel(HypixelApiService api)
    {
        _api = api;
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
        _refreshTimer.Tick += async (_, _) => await RefreshAsync();
    }

    private double CostPerKey =>
        _mithrilPlatePrice * 1 +
        _glaciteAmalgamationPrice * 2 +
        _refinedTungstenPrice * 4 +
        _refinedUmberPrice * 4 +
        _glaciteJewelPrice * 3;

    private double ProfitPerKey => _skeletonKeySellPrice - CostPerKey;

    public string MithrilPlateDisplay    => FormatPrice(_mithrilPlatePrice);
    public string AmalgamationDisplay    => FormatPrice(_glaciteAmalgamationPrice);
    public string RefinedTungstenDisplay => FormatPrice(_refinedTungstenPrice);
    public string RefinedUmberDisplay    => FormatPrice(_refinedUmberPrice);
    public string GlaciteJewelDisplay    => FormatPrice(_glaciteJewelPrice);
    public string SkeletonKeySellDisplay => FormatPrice(_skeletonKeySellPrice);

    public string CostX1  => FormatPrice(CostPerKey);
    public string CostX7  => FormatPrice(CostPerKey * SlotsPerPerson);
    public string CostX35 => FormatPrice(CostPerKey * SlotsPerPerson * MembersCount);

    public string ProfitX1  => FormatProfit(ProfitPerKey);
    public string ProfitX7  => FormatProfit(ProfitPerKey * SlotsPerPerson);
    public string ProfitX35 => FormatProfit(ProfitPerKey * SlotsPerPerson * MembersCount);

    public string ProfitPerHourX1  => FormatProfit(ProfitPerKey / CycleHours);
    public string ProfitPerHourX7  => FormatProfit(ProfitPerKey * SlotsPerPerson / CycleHours);
    public string ProfitPerHourX35 => FormatProfit(ProfitPerKey * SlotsPerPerson * MembersCount / CycleHours);

    public string ProfitColorX1  => ProfitPerKey >= 0 ? "#4CAF50" : "#EF4444";
    public string ProfitColorX7  => ProfitPerKey >= 0 ? "#4CAF50" : "#EF4444";
    public string ProfitColorX35 => ProfitPerKey >= 0 ? "#4CAF50" : "#EF4444";

    private string _lastUpdate = "Jamais";
    public string LastUpdate
    {
        get => _lastUpdate;
        set { _lastUpdate = value; OnPropertyChanged(); }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            var products = await _api.GetBazaarAsync();

            _mithrilPlatePrice        = GetBuyPrice(products, "MITHRIL_PLATE");
            _glaciteAmalgamationPrice = GetBuyPrice(products, "GLACITE_AMALGAMATION");
            _refinedTungstenPrice     = GetBuyPrice(products, "REFINED_TUNGSTEN");
            _refinedUmberPrice        = GetBuyPrice(products, "REFINED_UMBER");
            _glaciteJewelPrice        = GetBuyPrice(products, "GLACITE_JEWEL");
            _skeletonKeySellPrice     = GetBuyPrice(products, "SKELETON_KEY");

            LastUpdate = $"Mis à jour à {DateTime.Now:HH:mm:ss}";
            NotifyAll();
        }
        catch { }
        finally { IsLoading = false; }
    }

    private static double GetBuyPrice(Dictionary<string, BazaarItem> products, string key)
        => products.TryGetValue(key, out var item) ? item.QuickStatus.BuyPrice : 0;

    private static string FormatPrice(double price)
        => price == 0 ? "—" : $"{price:N0} coins";

    private static string FormatProfit(double profit)
        => profit == 0 ? "—" : $"{(profit >= 0 ? "+" : "")}{profit:N0} coins";

    private void NotifyAll()
    {
        OnPropertyChanged(nameof(MithrilPlateDisplay));
        OnPropertyChanged(nameof(AmalgamationDisplay));
        OnPropertyChanged(nameof(RefinedTungstenDisplay));
        OnPropertyChanged(nameof(RefinedUmberDisplay));
        OnPropertyChanged(nameof(GlaciteJewelDisplay));
        OnPropertyChanged(nameof(SkeletonKeySellDisplay));
        OnPropertyChanged(nameof(CostX1));
        OnPropertyChanged(nameof(CostX7));
        OnPropertyChanged(nameof(CostX35));
        OnPropertyChanged(nameof(ProfitX1));
        OnPropertyChanged(nameof(ProfitX7));
        OnPropertyChanged(nameof(ProfitX35));
        OnPropertyChanged(nameof(ProfitPerHourX1));
        OnPropertyChanged(nameof(ProfitPerHourX7));
        OnPropertyChanged(nameof(ProfitPerHourX35));
        OnPropertyChanged(nameof(ProfitColorX1));
        OnPropertyChanged(nameof(ProfitColorX7));
        OnPropertyChanged(nameof(ProfitColorX35));
    }

    public void StartTimer() => _refreshTimer.Start();
    public void StopTimer()  => _refreshTimer.Stop();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}