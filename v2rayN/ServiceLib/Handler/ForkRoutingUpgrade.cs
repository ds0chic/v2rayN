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
        new("绕过大陆(Whitelist)", "white", "3A18A88FC66DFDBE38A161C91CC71ED9DE7AD8478D505FE31E7BA789E11A5B16", ["7FD1E27371EAA900CB1CE01202E94BFA8F9A0215C6A670402FCF5B47479F60DC", "DA25548D7E17A550A2726235D17BF1E022A275A2A8950AD830CCAA39AB2C4A6C", "1C73C862218FD8445500A436521A41AF69F5B0B3E486EBFC91783F9BA922FE6D", "FD414FA0ADFD826D9742BC0FEAAC0BA22903CD6CE57DD9746D3A50C752263788", "FDBF95ECEFBF6B0C8397A91EB7FA83F8E2446975008BE6497CF41C82463C5031", "B5127DEC5062543464B8B2114E8A7E74F0C8DD98C0C611CC2275AE045E6AC426", "E9A4EAD72E3B7157BB7BAB0F2533EB10D284859611D2585F4B3AE4D7DDD6554E", "242B209E294F980B62489E57317483E4BB721803C675288AA08EEF4DDDC6FCF6"], Global.IPIfNonMatch),
        new("黑名单(Blacklist)", "black", "035DAD4F1C17B168456995FB773B9A17489BAB68BE0308E55D0F55EED9DCC153", ["B85620E70B6494FF53290FC752731A62C76B576C4F8A25A1DC945C130785D275", "C5F6A989E2DE82B8FD04DD6949E0778D54C27D7480FFFB106B486EC57BFA3C1A", "7FD1E27371EAA900CB1CE01202E94BFA8F9A0215C6A670402FCF5B47479F60DC", "DA25548D7E17A550A2726235D17BF1E022A275A2A8950AD830CCAA39AB2C4A6C", "1C73C862218FD8445500A436521A41AF69F5B0B3E486EBFC91783F9BA922FE6D", "FD414FA0ADFD826D9742BC0FEAAC0BA22903CD6CE57DD9746D3A50C752263788", "644B5C57B59003846983791436DAF6CF87B8C92F2D6AB843E5996A77A32D8146", "B59522D01D7177AC32B09EEB8C8953F0FF6C0B8A9E29D86C6180FDA1498DAD07", "FD478AF296FA78983D5FE548A83B8D18AC8D23F0FBA0A47A0371221A3558CC1F", "FA65269CD8A24EDEE0A56CDEA5AAB19D19D4E146BA2F9B126D13C183DE31953A", "62AF273FC05DFA3A532A31F41A70D182443F7B59CBDCB09B2C6632529093A804"], Global.IPOnDemand),
        new("全局(Global)", "global", "F15C0D82CC357E0ACCB770E15F5830F2D0AF6959F9435FD30B7C7FA91638F22F", ["7FD1E27371EAA900CB1CE01202E94BFA8F9A0215C6A670402FCF5B47479F60DC", "1C73C862218FD8445500A436521A41AF69F5B0B3E486EBFC91783F9BA922FE6D", "FD414FA0ADFD826D9742BC0FEAAC0BA22903CD6CE57DD9746D3A50C752263788", "EBE7D8FEC2B707A88880D5D65A833FD425481D650D363661EF905BF94D585FE7"], string.Empty),
    ];

    // Behavior of one rule without its remarks or id, Enabled excluded.
    private static string RuleContent(RulesItem r)
    {
        return new StringBuilder().Append(r.OutboundTag).Append('|').Append(r.Port).Append('|').Append(r.Network).Append('|')
            .Append(string.Join(',', r.Protocol ?? [])).Append('|')
            .Append(string.Join(',', r.Domain ?? [])).Append('|')
            .Append(string.Join(',', r.Ip ?? [])).Append('|')
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
