using ServiceLib.Helper;

namespace ServiceLib.Handler;

// Fork-only: moves the three built-in routing sets (whitelist, blacklist, global) to the dual-stack, more precise templates.
// A set that still equals the original upstream template is replaced outright. An edited set is merged: its rules that are
// not part of the original template (user edits and user-added rules) are kept FIRST, in their original order, so the user's
// force-proxy rules still win over the new template rules that follow them.
public static class ForkRoutingUpgrade
{
    public const string Version = "V4V6-";
    private const string OldVersion = "V4-";

    private sealed record Builtin(string Suffix, string TemplateKey, string OldSignature, string[] OldRuleSignatures, string DomainStrategy);

    // OldSignature: whole-set digest of the original upstream template. OldRuleSignatures: per-rule digests of the same template.
    private static readonly Builtin[] Sets =
    [
        new("绕过大陆(Whitelist)", "white", "9C08759836583FD37678CA13545122FF4E2E87250AE1DFDFEE42EB14ED12C56A", ["EC9C807A3BA6868115398A0C0FEBB31D08C4EAC99E45A6FB8E768841AB47E1A5","3FA349D760BB48B16B83ABF5E150511E905BFCD26F0067D3DB7593A9C709DFFF","B3DABC0A11FA64FB5FBCC32BFADBCDBF8954363A52B29BE393BBD700C345112C","2462A8BD741C2AC4CA18D8E4BE6FB9C38F4FF44155887386EEDE7AED9BC60272","418F507255CAD6E3870E033C33C689CDF5325109DD00088A71E8CF0EC7580345","DCCD74696C45222392533C352B28B2C3617788AE7B9F6149A87525946B515BA0","7AE950AD6C0C1134909FC379587039544AF4B0342CF0F339C821FC30DA63E7EF","41E9247895A03284084839322E4EAA011CC61C640D8669CD652B3F124038D54E"], Global.IPIfNonMatch),
        new("黑名单(Blacklist)", "black", "6BE5EDD81B5534107B71E3E2A50E430DCD1AB9AA875020A5B2CD126B27C01FE7", ["C5C67698AA46FF3C3E887A50C33AFE962B0B5534B5F69B8AC6163752EC4B93C2","690AF6675DAD50CDA69376A7DF4630F442BA076C43752C4E581D43D4A953CCDC","EC9C807A3BA6868115398A0C0FEBB31D08C4EAC99E45A6FB8E768841AB47E1A5","3FA349D760BB48B16B83ABF5E150511E905BFCD26F0067D3DB7593A9C709DFFF","B3DABC0A11FA64FB5FBCC32BFADBCDBF8954363A52B29BE393BBD700C345112C","2462A8BD741C2AC4CA18D8E4BE6FB9C38F4FF44155887386EEDE7AED9BC60272","B67FEECF58C51BE71502366580DBFDB0DD686A65D7CCD0A5417BA91034F32510","BF9444C717B7D5F39B57AFE26875C95114AAADE921B99E6C695A88CFF3F630AA","BB56E82F511E525C871F0A9CC43371ABB0B9972C175906E3E5E7B6FDB661453A","932562F2F164CABE6143C0E33713818F54DA4E8BB34D19609A1FB7D40D6D981F","90303621F563178832BA24FC0D437507CE17992A899BBA4F6CE6C679594DFCA4"], Global.IPOnDemand),
        new("全局(Global)", "global", "967E6FE74A3ABCFB46D0161D22F1459E5B002B5EBC90681E67D3FC5D0EC2A72B", ["EC9C807A3BA6868115398A0C0FEBB31D08C4EAC99E45A6FB8E768841AB47E1A5","B3DABC0A11FA64FB5FBCC32BFADBCDBF8954363A52B29BE393BBD700C345112C","2462A8BD741C2AC4CA18D8E4BE6FB9C38F4FF44155887386EEDE7AED9BC60272","241F3EB206976382456CE29B615B946C9B94BBC5F9977D0609A3AA11AC4A88B8"], string.Empty),
    ];

    // Behavior of one rule without its remarks or id, Enabled excluded. Type, InboundTag, Process and RuleType are included
    // because the rule editor can change them; the original templates leave them empty, so null and [] must digest the same.
    private static string RuleContent(RulesItem r)
    {
        return new StringBuilder().Append(r.OutboundTag).Append('|').Append(r.Port).Append('|').Append(r.Network).Append('|')
            .Append(string.Join(',', r.Protocol ?? [])).Append('|')
            .Append(string.Join(',', r.Domain ?? [])).Append('|')
            .Append(string.Join(',', r.Ip ?? [])).Append('|')
            .Append(r.Type).Append('|')
            .Append(string.Join(',', r.InboundTag ?? [])).Append('|')
            .Append(string.Join(',', r.Process ?? [])).Append('|')
            .Append(r.RuleType).Append('|')
            .ToString();
    }

    private static string Digest(string text)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    }

    // Digest of one rule's behavior (remarks and ids do not count).
    public static string RuleSignature(RulesItem r)
    {
        return Digest(RuleContent(r) + r.Enabled + ";");
    }

    // Order-sensitive digest of what a rule set actually does (remarks and ids do not count).
    public static string Signature(IEnumerable<RulesItem>? rules)
    {
        var sb = new StringBuilder();
        foreach (var r in rules ?? [])
        {
            sb.Append(RuleContent(r)).Append(r.Enabled).Append(';');
        }

        return Digest(sb.ToString());
    }

    public static string SignatureOf(string templateJson)
    {
        return Signature(JsonUtils.Deserialize<List<RulesItem>>(templateJson));
    }

    public static string OldSignatureFor(string templateKey)
    {
        return Sets.First(t => t.TemplateKey == templateKey).OldSignature;
    }

    public static string[] OldRuleSignaturesFor(string templateKey)
    {
        return Sets.First(t => t.TemplateKey == templateKey).OldRuleSignatures;
    }

    public static string DomainStrategyFor(string templateKey)
    {
        return Sets.First(t => t.TemplateKey == templateKey).DomainStrategy;
    }

    public sealed record UpgradePlan(string TemplateKey, string Remarks, string DomainStrategy, List<RulesItem> Rules);

    // Pure decision, no database access. Null when the item is not an "V4-" built-in set (already upgraded, renamed or user's own).
    public static UpgradePlan? PlanUpgrade(RoutingItem item)
    {
        var set = Sets.FirstOrDefault(t => item.Remarks == OldVersion + t.Suffix);
        if (set is null || item.RuleSet.IsNullOrEmpty())
        {
            return null;
        }

        var rules = JsonUtils.Deserialize<List<RulesItem>>(item.RuleSet);
        if (rules is null)
        {
            return null;
        }

        var remarks = Version + set.Suffix;
        if (Signature(rules) == set.OldSignature)
        {
            return new UpgradePlan(set.TemplateKey, remarks, set.DomainStrategy, TemplateRules(set.TemplateKey));
        }

        var domainStrategy = item.DomainStrategy.IsNullOrEmpty() || item.DomainStrategy == Global.AsIs
            ? set.DomainStrategy
            : item.DomainStrategy;
        return new UpgradePlan(set.TemplateKey, remarks, domainStrategy, MergeRules(set, rules));
    }

    private static List<RulesItem> TemplateRules(string templateKey)
    {
        var rules = JsonUtils.Deserialize<List<RulesItem>>(EmbedUtils.GetEmbedText(Global.CustomRoutingFileName + templateKey)) ?? [];
        foreach (var r in rules)
        {
            r.Id = Utils.GetGuid(false);
        }

        return rules;
    }

    // Custom rules first (original order, Remarks/Enabled/Id kept), then the new template rules.
    private static List<RulesItem> MergeRules(Builtin set, List<RulesItem> current)
    {
        var oldSignatures = set.OldRuleSignatures.ToHashSet();
        var template = TemplateRules(set.TemplateKey);
        var templateSignatures = template.Select(RuleSignature).ToHashSet();

        // A custom rule is one the original template does not contain. An enabled custom rule identical to a template rule is
        // dropped (the template copy covers it); a disabled one is kept because the user turned it off.
        var custom = current
            .Where(r => !oldSignatures.Contains(RuleSignature(r)))
            .Where(r => !(r.Enabled && templateSignatures.Contains(RuleSignature(r))))
            .ToList();
        foreach (var r in custom.Where(r => string.IsNullOrEmpty(r.Id)))
        {
            r.Id = Utils.GetGuid(false);
        }

        // A template rule whose disabled custom copy exists is dropped: the user turned that rule off.
        var disabledContents = custom.Where(r => !r.Enabled).Select(RuleContent).ToHashSet();
        var merged = new List<RulesItem>(custom);
        merged.AddRange(template.Where(t => !disabledContents.Contains(RuleContent(t))));
        return merged;
    }

    public static async Task UpgradeAsync(IEnumerable<RoutingItem> items)
    {
        foreach (var item in items.Where(t => t.Remarks.StartsWith(OldVersion)).ToList())
        {
            var plan = PlanUpgrade(item);
            if (plan is null)
            {
                continue;
            }

            item.Remarks = plan.Remarks;
            item.DomainStrategy = plan.DomainStrategy;
            item.RuleSet = JsonUtils.Serialize(plan.Rules, false);
            item.RuleNum = plan.Rules.Count;
            if (item.Id.IsNullOrEmpty())
            {
                item.Id = Utils.GetGuid(false);
            }

            await SQLiteHelper.Instance.ReplaceAsync(item);
        }
    }
}
