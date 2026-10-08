namespace ServiceLib.Common;

public enum LanShareState
{
    Off,
    Recommended,
    NoAuth,
    SharedPort,
}

// Fork-only LAN sharing rules, free of UI code. Mirrors the rules in V2rayInboundService.GenInbounds.
public static class LanShare
{
    private const string RandomChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int RandomUserLength = 8;
    private const int RandomPassLength = 16;

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
}
