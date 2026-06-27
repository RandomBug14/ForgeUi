using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ForgeUI.ViewModels;

namespace ForgeUI.Views;

public partial class MiniBarWindow : Window
{
    private static readonly HttpClient _http = new();
    private readonly Dictionary<string, Ellipse> _leds = new();

    public MiniBarWindow()
    {
        InitializeComponent();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => DragMove();

    public async Task InitMembersAsync(IEnumerable<MemberForgeViewModel> members)
    {
        MembersPanel.Children.Clear();
        _leds.Clear();

        foreach (var member in members)
        {
            var grid = new Grid();

            // Fond coloré par défaut avec initiales
            var fallback = new Border
            {
                Width = 42,
                Height = 42,
                Background = GetAvatarBackground(member.MemberUuid),
                CornerRadius = new CornerRadius(21)
            };
            var initials = new TextBlock
            {
                Text = GetInitials(member.DisplayName),
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            fallback.Child = initials;
            grid.Children.Add(fallback);

            // Tentative de chargement du skin par dessus
            try
            {
                var uuidClean = member.MemberUuid.Replace("-", "");
                var bytes = await _http.GetByteArrayAsync(
                    $"https://crafatar.com/avatars/{uuidClean}?size=42&overlay");

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new System.IO.MemoryStream(bytes);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                var avatar = new Image
                {
                    Width = 42,
                    Height = 42,
                    Source = bmp,
                    Stretch = Stretch.UniformToFill,
                    ClipToBounds = true
                };
                grid.Children.Add(avatar); // par dessus le fallback
            }
            catch { } // si ça échoue, les initiales restent visibles

            // LED
            var led = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = new SolidColorBrush(GetLedColor(member)),
                Stroke = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11)),
                StrokeThickness = 1.5,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, -1, -1)
            };
            grid.Children.Add(led);
            _leds[member.MemberUuid] = led;

            var container = new Border
            {
                Width = 42,
                Height = 42,
                CornerRadius = new CornerRadius(21),
                Margin = new Thickness(3, 0, 3, 0),
                ToolTip = BuildTooltip(member),
                Child = grid
            };

            MembersPanel.Children.Add(container);
        }
    }

    public void RefreshLeds(IEnumerable<MemberForgeViewModel> members)
    {
        foreach (var member in members)
        {
            if (_leds.TryGetValue(member.MemberUuid, out var led))
                led.Fill = new SolidColorBrush(GetLedColor(member));
        }
    }

    private static string GetInitials(string displayName)
    {
        // Retire l'emoji 👑 si présent
        var clean = displayName.Replace("👑 ", "").Trim();
        return clean.Length >= 2 ? clean[..2].ToUpper() : clean.ToUpper();
    }

private static readonly Color[] _palette =
[
    Color.FromRgb(0x6B, 0x48, 0xFF), // violet
    Color.FromRgb(0xFF, 0x6B, 0x35), // orange
    Color.FromRgb(0x35, 0x9B, 0xFF), // bleu
    Color.FromRgb(0xFF, 0x35, 0x8A), // rose
    Color.FromRgb(0x43, 0xa0, 0x47), // vert
];

private static readonly Dictionary<string, Color> _assignedColors = new();
private static int _nextColorIndex = 0;

private static SolidColorBrush GetAvatarBackground(string uuid)
{
    if (!_assignedColors.TryGetValue(uuid, out var color))
    {
        color = _palette[_nextColorIndex % _palette.Length];
        _nextColorIndex++;
        _assignedColors[uuid] = color;
    }
    return new SolidColorBrush(color);
}

    private static Color GetLedColor(MemberForgeViewModel member)
    {
        if (member.TotalCount == 0)
            return Color.FromRgb(0xEF, 0x44, 0x44);

        if (member.DoneCount == member.TotalCount)
            return Color.FromRgb(0x4C, 0xAF, 0x50);

        return Color.FromRgb(0xFF, 0xA5, 0x00);
    }
    private static string BuildTooltip(MemberForgeViewModel member)
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine(member.DisplayName);
    sb.AppendLine($"{member.Summary}");
    sb.AppendLine("─────────────────");

    if (member.Slots.Count == 0)
    {
        sb.AppendLine("Forge vide");
    }
    else
    {
        foreach (var slot in member.Slots.OrderBy(s => s.SlotNumber))
        {
            var status = slot.IsDone ? "✔" : slot.IsUnknownDuration ? "?" : "⏳";
            sb.AppendLine($"{status} Slot {slot.SlotNumber} — {slot.ItemDisplayName}");
            //sb.AppendLine($"    {slot.RemainingDisplay}");
        }
    }

    return sb.ToString().TrimEnd();
}
}