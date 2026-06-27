using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ForgeUI.Converters;

public class ProgressToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            if (d >= 100) return new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // Vert
            if (d >= 75)  return new SolidColorBrush(Color.FromRgb(0x8B, 0xC3, 0x4A)); // Vert clair
            if (d >= 50)  return new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07)); // Jaune
            if (d >= 25)  return new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)); // Orange
            return new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));              // Rouge
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true
            ? System.Windows.Visibility.Visible
            : System.Windows.Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class StringToColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hex)
        {
            try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)); }
            catch { }
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
