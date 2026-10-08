using Microsoft.Win32;

namespace ServiceLib.Common;

[SupportedOSPlatform("windows")]
internal static class WindowsUtils
{
    private static readonly string _tag = "WindowsUtils";

    public static string? RegReadValue(string path, string name, string def)
    {
        RegistryKey? regKey = null;
        try
        {
            regKey = Registry.CurrentUser.OpenSubKey(path, false);
            var value = regKey?.GetValue(name) as string;
            return value.IsNullOrEmpty() ? def : value;
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
        finally
        {
            regKey?.Close();
        }
        return def;
    }

    public static void RegWriteValue(string path, string name, object value)
    {
        RegistryKey? regKey = null;
        try
        {
            regKey = Registry.CurrentUser.CreateSubKey(path);
            if (value.ToString().IsNullOrEmpty())
            {
                regKey?.DeleteValue(name, false);
            }
            else
            {
                regKey?.SetValue(name, value);
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
        finally
        {
            regKey?.Close();
        }
    }

    public static Guid GetTunDeviceGuid(string deviceName)
    {
        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(deviceName)));
    }

    public static async Task<bool> IsTunDevicePresent(string tunName)
    {
        try
        {
            var guid = GetTunDeviceGuid(tunName);
            var pnpUtilPath = @"C:\Windows\System32\pnputil.exe";
            var arg = $$""" /enum-devices /instanceid "SWD\Wintun\{{{guid}}}" """;

            var output = await Utils.GetCliWrapOutput(pnpUtilPath, arg);
            return output?.Contains(guid.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
        }
        catch
        {
            return false;
        }
    }

    public static async Task RemoveTunDevice()
    {
        var tunNameList = new List<string> { "wintunsingbox_tun", "xray_tun" };
        foreach (var tunName in tunNameList)
        {
            try
            {
                if (!await IsTunDevicePresent(tunName))
                {
                    continue;
                }

                var guid = GetTunDeviceGuid(tunName);
                var pnpUtilPath = @"C:\Windows\System32\pnputil.exe";
                var arg = $$""" /remove-device  "SWD\Wintun\{{{guid}}}" """;

                _ = await Utils.GetCliWrapOutput(pnpUtilPath, arg);

                var stopTime = DateTime.UtcNow.AddSeconds(3);
                while (DateTime.UtcNow < stopTime)
                {
                    if (!await IsTunDevicePresent(tunName))
                    {
                        break;
                    }
                    await Task.Delay(200);
                }

                if (await IsTunDevicePresent(tunName))
                {
                    Logging.SaveLog($"TUN device {tunName} still present after removal");
                }
            }
            catch (Exception ex)
            {
                Logging.SaveLog(_tag, ex);
            }
        }
    }
}
