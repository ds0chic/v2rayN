using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace v2rayN.Desktop.Common;

// Hides a window from the user with DWMWA_CLOAK while it keeps rendering, so showing it again from the tray
// is instant (no re-created render target, no empty frame). No-op on other platforms.
internal static partial class WindowCloakHelper
{
    private const int DwmCloak = 13;

    // Returns false when the window could not be (un)cloaked; callers should then fall back to Hide()/Show().
    public static bool TrySetCloaked(Window window, bool cloaked)
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        var hwnd = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero)
        {
            return false;
        }

        var value = cloaked ? 1 : 0;
        return DwmSetWindowAttribute(hwnd, DwmCloak, ref value, sizeof(int)) == 0;
    }

    [SupportedOSPlatform("windows")]
    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
}
