using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace v2rayN.Desktop.Common;

public sealed record LanShareAddress(string Name, string Ip, bool IsVirtual);

// Fork-only UI helpers for the LAN sharing card (adapters, netsh). Pure LAN rules live in ServiceLib.Common.LanShare.
public static class LanShareHelper
{
    private static readonly string[] VirtualNameKeywords =
    [
        "vEthernet", "VMware", "VirtualBox", "Hyper-V", "Docker", "WSL", "TAP-Windows", "Wintun", "TUN", "Loopback",
    ];

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

    // Runs netsh and returns a user-facing result. Caller must check administrator rights first.
    public static async Task<string> AddFirewallRuleAsync(int port)
    {
        try
        {
            // Replace an existing rule with the same name instead of stacking duplicates.
            // Delete fails when no rule exists yet; that is expected, so its result is ignored.
            await RunNetshAsync("advfirewall", "firewall", "delete", "rule", "name=v2rayN LAN proxy");

            var result = await RunNetshAsync(
                "advfirewall", "firewall", "add", "rule", "name=v2rayN LAN proxy",
                "dir=in", "action=allow", "protocol=TCP", $"localport={port}");
            if (result is null)
            {
                return ForkText.LanFirewallFailed;
            }

            var (exitCode, output) = result.Value;
            return exitCode == 0
                ? $"{ForkText.LanFirewallSuccess}\n{output}"
                : $"{ForkText.LanFirewallFailed} (exit {exitCode})\n{output}";
        }
        catch (Exception ex)
        {
            Logging.SaveLog("AddFirewallRuleAsync", ex);
            return $"{ForkText.LanFirewallFailed}\n{ex.Message}";
        }
    }

    private static async Task<(int ExitCode, string Output)?> RunNetshAsync(params string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "netsh",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return null;
        }

        var stderrTask = process.StandardError.ReadToEndAsync();
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await stderrTask;
        await process.WaitForExitAsync();

        return (process.ExitCode, (stdout + stderr).Trim());
    }

    private static bool IsVirtualName(string? name)
    {
        return !string.IsNullOrEmpty(name)
               && VirtualNameKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}
