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
            <= 0 => new SolidColorBrush(Color.Parse(dark ? "#E08A8A" : "#B04A4A")),
            <= 500 => new SolidColorBrush(Color.Parse(dark ? "#8DBF9A" : "#4A8A5C")),
            _ => new SolidColorBrush(Color.Parse(dark ? "#D9B26A" : "#A8741A"))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
