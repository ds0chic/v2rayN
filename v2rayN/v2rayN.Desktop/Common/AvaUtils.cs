using Avalonia.Input.Platform;

namespace v2rayN.Desktop.Common;

internal class AvaUtils
{
    public static async Task<string?> GetClipboardData(Window owner)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(owner)?.Clipboard;
            if (clipboard == null)
            {
                return null;
            }

            return await clipboard.TryGetTextAsync();
        }
        catch
        {
            return null;
        }
    }

    public static async Task SetClipboardData(Visual? visual, string strData)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(visual)?.Clipboard;
            if (clipboard == null)
            {
                return;
            }

            await clipboard.SetTextAsync(strData);
        }
        catch
        {
        }
    }

    public static Bitmap GetAppIconBitmap(ESysProxyType sysProxyType)
    {
        var index = (int)sysProxyType + 1;
        var fileName = Utils.GetPath($"NotifyIcon{index}.ico");
        if (File.Exists(fileName))
        {
            return new Bitmap(fileName);
        }

        return new Bitmap(AssetLoader.Open(new Uri(Path.Combine(Global.AvaAssets, $"NotifyIcon{index}.ico"))));
    }

    /// <summary>
    /// Tray icon for a system proxy mode: the creature fills the icon and the mode's prop is a badge in the corner
    /// (Assets/TrayIcon1-4.ico). A user's own NotifyIcon{n}.ico next to the program still wins.
    /// </summary>
    public static WindowIcon GetTrayIcon(ESysProxyType sysProxyType)
    {
        var index = (int)sysProxyType + 1;
        var fileName = Utils.GetPath($"NotifyIcon{index}.ico");
        if (File.Exists(fileName))
        {
            return new(fileName);
        }

        var uri = new Uri(Path.Combine(Global.AvaAssets, $"TrayIcon{index}.ico"));
        using var bitmap = new Bitmap(AssetLoader.Open(uri));
        return new(bitmap);
    }

    public static WindowIcon GetAppIcon(ESysProxyType sysProxyType)
    {
        var index = (int)sysProxyType + 1;
        var fileName = Utils.GetPath($"NotifyIcon{index}.ico");
        if (File.Exists(fileName))
        {
            return new(fileName);
        }

        var uri = new Uri(Path.Combine(Global.AvaAssets, $"NotifyIcon{index}.ico"));
        using var bitmap = new Bitmap(AssetLoader.Open(uri));
        return new(bitmap);
    }
}
