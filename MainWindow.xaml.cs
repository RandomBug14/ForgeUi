using System.Windows;
using System.Windows.Input;
using ForgeUI.Services;
using ForgeUI.ViewModels;
using ForgeUI.Views;

namespace ForgeUI;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private MiniBarWindow? _miniBar;

    public MainWindow()
    {
        InitializeComponent();

        var config = ConfigService.Load();
        var api = new HypixelApiService(config);
        _vm = new MainViewModel(api, config);

        _vm.RefreshCommand = new RelayCommand(
            async _ => await _vm.LoadDataAsync(),
            _ => _vm.CanRefresh
        );

        DataContext = _vm;
    }

protected override async void OnContentRendered(EventArgs e)
{
    base.OnContentRendered(e);
    _vm.StartTimers();
    await _vm.LoadDefaultPlayerAsync();

    _vm.OnMembersRefreshed += () =>
{
    if (_miniBar?.IsVisible == true)
        _miniBar.RefreshLeds(_vm.Members);
};

_vm.OnDataLoaded += async () =>
{
    if (_miniBar?.IsVisible == true)
        await _miniBar.InitMembersAsync(_vm.Members);
};
}

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        await _vm.SearchPlayerAsync(_vm.SearchText);
    }

    private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await _vm.SearchPlayerAsync(_vm.SearchText);
    }

    private async void ToggleMiniBar_Click(object sender, RoutedEventArgs e)
    {
        if (_miniBar == null || !_miniBar.IsVisible)
        {
            _miniBar ??= new MiniBarWindow();
            _miniBar.Show();
            await _miniBar.InitMembersAsync(_vm.Members);
        }
        else
        {
            _miniBar.Hide();
        }
    }

    private void PinButton_Checked(object sender, RoutedEventArgs e) => Topmost = true;
    private void PinButton_Unchecked(object sender, RoutedEventArgs e) => Topmost = false;

    protected override void OnClosed(EventArgs e)
    {
        _miniBar?.Close();
        _vm.StopTimers();
        base.OnClosed(e);
    }
}

public class RelayCommand : ICommand
{
    private readonly Func<object?, Task> _executeAsync;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isExecuting;

    public RelayCommand(Func<object?, Task> executeAsync, Func<object?, bool>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
        => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;
        _isExecuting = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try { await _executeAsync(parameter); }
        finally
        {
            _isExecuting = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}