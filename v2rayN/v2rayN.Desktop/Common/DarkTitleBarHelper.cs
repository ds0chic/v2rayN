using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace v2rayN.Desktop.Common;

// Sets DWMWA_USE_IMMERSIVE_DARK_MODE so the native Windows title bar follows the app theme. No-op on other platforms.
internal static partial class DarkTitleBarHelper
{
    private const int DwmUseImmersiveDarkModeBefore20H1 = 19;
    private const int DwmUseImmersiveDarkMode = 20;

    public static void ApplyToAllWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var window in desktop.Windows)
            {
                Apply(window);
            }
        }
    }

    public static void Apply(Window window)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var hwnd = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var dark = IsDark(window.ActualThemeVariant) ? 1 : 0;
        if (DwmSetWindowAttribute(hwnd, DwmUseImmersiveDarkMode, ref dark, sizeof(int)) != 0)
        {
            DwmSetWindowAttribute(hwnd, DwmUseImmersiveDarkModeBefore20H1, ref dark, sizeof(int));
        }
    }

    // Semi variants (Dusk, NightSky, ...) are custom ThemeVariants that inherit from Dark or Light.
    private static bool IsDark(ThemeVariant? variant)
    {
        for (var v = variant; v is not null; v = v.InheritVariant)
        {
            if (v == ThemeVariant.Dark)
            {
                return true;
            }

            if (v == ThemeVariant.Light)
            {
                return false;
            }
        }

        return false;
    }

    [SupportedOSPlatform("windows")]
    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
}
