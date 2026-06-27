using System.Windows;

namespace ForgeUI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Gestion globale des exceptions non catchées
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(
                $"Erreur inattendue :\n{args.Exception.Message}",
                "Skyblock Forge Tracker",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}
