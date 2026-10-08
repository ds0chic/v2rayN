using System.Security.Cryptography;

namespace ServiceLib.Manager;

/// <summary>
/// Secret that protects the local sing-box/mihomo external controller (Clash API).
/// A random value is generated once per app run. A secret set by the user in a custom mihomo config is kept.
/// </summary>
public static class ClashApiSecret
{
    private static readonly string _generated = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private static volatile string _active = _generated;

    /// <summary>Secret that Clash API requests must present for the most recently generated core config.</summary>
    public static string Active => _active;

    /// <summary>Returns the secret to write into a generated core config and makes it the active one.</summary>
    public static string Apply(string? userSecret = null)
    {
        var secret = string.IsNullOrEmpty(userSecret) ? _generated : userSecret;
        _active = secret;
        return secret;
    }
}
