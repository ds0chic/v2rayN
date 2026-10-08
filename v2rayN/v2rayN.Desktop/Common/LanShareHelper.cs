using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace v2rayN.Desktop.Common;

public enum LanShareState
{
    Off,
    Recommended,
    NoAuth,
    SharedPort,
}

public sealed record LanShareAddress(string Name, string Ip, bool IsVirtual);

// Fork-only helpers for the LAN sharing card. Mirrors the rules in V2rayInboundService.GenInbounds.
public static class LanShareHelper
{
    private const string RandomChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int RandomUserLength = 8;
    private const int RandomPassLength = 16;

    private static readonly string[] VirtualNameKeywords =
    [
        "vEthernet", "VMware", "VirtualBox", "Hyper-V", "Docker", "WSL", "TAP-Windows", "Wintun", "TUN", "Loopback",
    ];

    public static int GetLanPort(int localPort, bool newPort4Lan)
    {
        return newPort4Lan ? localPort + (int)EInboundProtocol.socks3 : localPort;
    }

    public static bool HasCredentials(string? user, string? pass)
    {
        return user.IsNotEmpty() && pass.IsNotEmpty();
    }

    // Core only applies auth on the dedicated LAN port.
    public static bool AuthApplies(bool allowLan, bool newPort4Lan, string? user, string? pass)
    {
        return allowLan && newPort4Lan && HasCredentials(user, pass);
    }

    public static LanShareState GetState(bool allowLan, bool newPort4Lan, string? user, string? pass)
    {
        if (!allowLan)
        {
            return LanShareState.Off;
        }

        if (!newPort4Lan)
        {
            return LanShareState.SharedPort;
        }

        return HasCredentials(user, pass) ? LanShareState.Recommended : LanShareState.NoAuth;
    }

    public static string BuildProxyUrl(string scheme, string ip, int port, string? user, string? pass, bool withAuth, bool maskPassword = false)
    {
        var auth = string.Empty;
        if (withAuth)
        {
            var shownPass = maskPassword ? "******" : Uri.EscapeDataString(pass!);
            auth = $"{Uri.EscapeDataString(user!)}:{shownPass}@";
        }

        return $"{scheme}://{auth}{ip}:{port}";
    }

    public static string GenerateRandomString(int length)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = RandomChars[RandomNumberGenerator.GetInt32(RandomChars.Length)];
        }

        return new string(chars);
    }

    public static string GenerateRandomUser() => GenerateRandomString(RandomUserLength);

    public static string GenerateRandomPass() => GenerateRandomString(RandomPassLength);

    public static List<LanShareAddress> GetLanIPv4Addresses()
    {
        var list = new List<LanShareAddress>();
        try
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up
                    || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                var isVirtual = nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel
                                || IsVirtualName(nic.Name)
                                || IsVirtualName(nic.Description);

                foreach (var unicast in nic.GetIPProperties().UnicastAddresses)
                {
                    var ip = unicast.Address;
                    if (ip.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(ip))
                    {
                        continue;
                    }

                    var bytes = ip.GetAddressBytes();
                    if (bytes[0] == 169 && bytes[1] == 254)
                    {
                        continue;
                    }

                    list.Add(new LanShareAddress(nic.Name, ip.ToString(), isVirtual));
                }
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog("GetLanIPv4Addresses", ex);
        }

        // Physical adapters first; OrderBy is stable so the original adapter order is kept within each group.
        return list.OrderBy(t => t.IsVirtual).ToList();
    }

    private static bool IsVirtualName(string? name)
    {
        return !string.IsNullOrEmpty(name)
               && VirtualNameKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}
