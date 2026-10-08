using System.Security.Cryptography;
using System.Text;

namespace ServiceLib.Handler;

// Fork-only: moves the three built-in routing sets (whitelist, blacklist, global) to the dual-stack, more precise
// templates. A set is replaced only while it still equals the original upstream template; edited sets are left alone.
public static class ForkRoutingUpgrade
{
    public const string Version = "V4V6-";
    private const string OldVersion = "V4-";

    private sealed record Builtin(string Suffix, string TemplateKey, string OldSignature, string DomainStrategy);

    private static readonly Builtin[] Sets =
    [
        new("绕过大陆(Whitelist)", "white", "3A18A88FC66DFDBE38A161C91CC71ED9DE7AD8478D505FE31E7BA789E11A5B16", Global.IPIfNonMatch),
        new("黑名单(Blacklist)", "black", "035DAD4F1C17B168456995FB773B9A17489BAB68BE0308E55D0F55EED9DCC153", Global.IPOnDemand),
        new("全局(Global)", "global", "F15C0D82CC357E0ACCB770E15F5830F2D0AF6959F9435FD30B7C7FA91638F22F", string.Empty),
    ];

    // Order-sensitive digest of what a rule set actually does (remarks and ids do not count).
    public static string Signature(IEnumerable<RulesItem>? rules)
    {
        var sb = new StringBuilder();
        foreach (var r in rules ?? [])
        {
            sb.Append(r.OutboundTag).Append('|').Append(r.Port).Append('|').Append(r.Network).Append('|')
              .Append(string.Join(',', r.Protocol ?? [])).Append('|')
              .Append(string.Join(',', r.Domain ?? [])).Append('|')
              .Append(string.Join(',', r.Ip ?? [])).Append('|')
              .Append(r.Enabled).Append(';');
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
    }

    public static string SignatureOf(string templateJson)
    {
        return Signature(JsonUtils.Deserialize<List<RulesItem>>(templateJson));
    }

    public static string OldSignatureFor(string templateKey)
    {
        return Sets.First(t => t.TemplateKey == templateKey).OldSignature;
    }

    public static string DomainStrategyFor(string templateKey)
    {
        return Sets.First(t => t.TemplateKey == templateKey).DomainStrategy;
    }

    // Returns the template key ("white", "black", "global") when the item is an unmodified original upstream set, otherwise null.
    public static string? MatchUnmodifiedOld(RoutingItem item)
    {
        var set = Sets.FirstOrDefault(t => item.Remarks == OldVersion + t.Suffix);
        if (set is null || item.RuleSet.IsNullOrEmpty())
        {
            return null;
        }

        var rules = JsonUtils.Deserialize<List<RulesItem>>(item.RuleSet);
        return Signature(rules) == set.OldSignature ? set.TemplateKey : null;
    }

    public static async Task UpgradeAsync(IEnumerable<RoutingItem> items)
    {
        foreach (var item in items.Where(t => t.Remarks.StartsWith(OldVersion)).ToList())
        {
            var key = MatchUnmodifiedOld(item);
            if (key is null)
            {
                continue;
            }

            var set = Sets.First(t => t.TemplateKey == key);
            item.Remarks = Version + set.Suffix;
            item.DomainStrategy = set.DomainStrategy;
            await ConfigHandler.AddBatchRoutingRules(item, EmbedUtils.GetEmbedText(Global.CustomRoutingFileName + key));
        }
    }
}
