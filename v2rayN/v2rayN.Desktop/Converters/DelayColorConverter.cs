using Avalonia.Data.Converters;

namespace v2rayN.Desktop.Converters;

public class DelayColorConverter : IValueConverter
{
    // fork: the stock Green / Red were too dark on the dark theme and unreadable on the green active row;
    // pick a lighter tint on dark and a deeper one on light.
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var delay = value.ToString().ToInt();
        var dark = Application.Current?.ActualThemeVariant != ThemeVariant.Light;

        return delay switch
        {
            <= 0 => new SolidColorBrush(Color.Parse(dark ? "#FCA5A5" : "#B91C1C")),
            <= 500 => new SolidColorBrush(Color.Parse(dark ? "#86EFAC" : "#15803D")),
            _ => new SolidColorBrush(Color.Parse(dark ? "#FDBA74" : "#C2410C"))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
