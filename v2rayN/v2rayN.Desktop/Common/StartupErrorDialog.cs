using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace v2rayN.Desktop.Common;

// Used before the Avalonia lifetime starts, so no Avalonia window exists yet.
// Windows gets a native message box (matching the WPF build); other platforms only log and write to stderr.
internal static partial class StartupErrorDialog
{
    private const uint MbOk = 0x0;
    private const uint MbIconError = 0x10;

    public static void Show(string message)
    {
        Logging.SaveLog(message);

        if (OperatingSystem.IsWindows())
        {
            MessageBoxW(IntPtr.Zero, message, Global.AppName, MbOk | MbIconError);
        }
        else
        {
            Console.Error.WriteLine(message);
        }
    }

    [SupportedOSPlatform("windows")]
    [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
