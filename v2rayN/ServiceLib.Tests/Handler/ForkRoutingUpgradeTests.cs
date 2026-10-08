using ServiceLib.Common;
using ServiceLib.Handler;
using ServiceLib.Models.Entities;

namespace ServiceLib.Tests.Handler;

public class ForkRoutingUpgradeTests
{
    // Original upstream "V4-" template: the signature of this text is the stored OldSignature.
    private const string OldWhite = """
        [
          {
            "remarks": "阻断udp443",
            "outboundTag": "block",
            "port": "443",
            "network": "udp"
          },
          {
            "remarks": "代理Google",
            "outboundTag": "proxy",
            "domain": [
              "geosite:google"
            ]
          },
          {
            "remarks": "绕过局域网IP",
            "outboundTag": "direct",
            "ip": [
              "geoip:private"
            ]
          },
          {
            "remarks": "绕过局域网域名",
            "outboundTag": "direct",
            "domain": [
              "geosite:private"
            ]
          },
          {
            "remarks": "绕过中国公共DNSIP",
            "outboundTag": "direct",
            "ip": [
              "223.5.5.5",
              "223.6.6.6",
              "2400:3200::1",
              "2400:3200:baba::1",
              "119.29.29.29",
              "1.12.12.12",
              "120.53.53.53",
              "2402:4e00::",
              "2402:4e00:1::",
              "180.76.76.76",
              "2400:da00::6666",
              "114.114.114.114",
              "114.114.115.115",
              "114.114.114.119",
              "114.114.115.119",
              "114.114.114.110",
              "114.114.115.110",
              "180.184.1.1",
              "180.184.2.2",
              "101.226.4.6",
              "218.30.118.6",
              "123.125.81.6",
              "140.207.198.6",
              "1.2.4.8",
              "210.2.4.8",
              "52.80.66.66",
              "117.50.22.22",
              "2400:7fc0:849e:200::4",
              "2404:c2c0:85d8:901::4",
              "117.50.10.10",
              "52.80.52.52",
              "2400:7fc0:849e:200::8",
              "2404:c2c0:85d8:901::8",
              "117.50.60.30",
              "52.80.60.30"
            ]
          },
          {
            "remarks": "绕过中国公共DNS域名",
            "outboundTag": "direct",
            "domain": [
              "domain:alidns.com",
              "domain:doh.pub",
              "domain:dot.pub",
              "domain:360.cn",
              "domain:onedns.net"
            ]
          },
          {
            "remarks": "绕过中国IP",
            "outboundTag": "direct",
            "ip": [
              "geoip:cn"
            ]
          },
          {
            "remarks": "绕过中国域名",
            "outboundTag": "direct",
            "domain": [
              "geosite:cn"
            ]
          }
        ]
        """;

    // Original upstream "V4-" template: the signature of this text is the stored OldSignature.
    private const string OldBlack = """
        [
          {
            "remarks": "绕过bittorrent",
            "outboundTag": "direct",
            "protocol": [
              "bittorrent"
            ]
          },
          {
            "remarks": "api.ip.sb",
            "outboundTag": "proxy",
            "domain": [
              "api.ip.sb"
            ]
          },
          {
            "remarks": "阻断udp443",
            "outboundTag": "block",
            "port": "443",
            "network": "udp"
          },
          {
            "remarks": "代理Google",
            "outboundTag": "proxy",
            "domain": [
              "geosite:google"
            ]
          },
          {
            "remarks": "绕过局域网IP",
            "outboundTag": "direct",
            "ip": [
              "geoip:private"
            ]
          },
          {
            "remarks": "绕过局域网域名",
            "outboundTag": "direct",
            "domain": [
              "geosite:private"
            ]
          },
          {
            "remarks": "代理海外公共DNSIP",
            "outboundTag": "proxy",
            "ip": [
              "1.1.1.1",
              "1.0.0.1",
              "2606:4700:4700::1111",
              "2606:4700:4700::1001",
              "1.1.1.2",
              "1.0.0.2",
              "2606:4700:4700::1112",
              "2606:4700:4700::1002",
              "1.1.1.3",
              "1.0.0.3",
              "2606:4700:4700::1113",
              "2606:4700:4700::1003",
              "8.8.8.8",
              "8.8.4.4",
              "2001:4860:4860::8888",
              "2001:4860:4860::8844",
              "94.140.14.14",
              "94.140.15.15",
              "2a10:50c0::ad1:ff",
              "2a10:50c0::ad2:ff",
              "94.140.14.15",
              "94.140.15.16",
              "2a10:50c0::bad1:ff",
              "2a10:50c0::bad2:ff",
              "94.140.14.140",
              "94.140.14.141",
              "2a10:50c0::1:ff",
              "2a10:50c0::2:ff",
              "208.67.222.222",
              "208.67.220.220",
              "2620:119:35::35",
              "2620:119:53::53",
              "208.67.222.123",
              "208.67.220.123",
              "2620:119:35::123",
              "2620:119:53::123",
              "9.9.9.9",
              "149.112.112.112",
              "2620:fe::9",
              "2620:fe::fe",
              "9.9.9.11",
              "149.112.112.11",
              "2620:fe::11",
              "2620:fe::fe:11",
              "9.9.9.10",
              "149.112.112.10",
              "2620:fe::10",
              "2620:fe::fe:10",
              "77.88.8.8",
              "77.88.8.1",
              "2a02:6b8::feed:0ff",
              "2a02:6b8:0:1::feed:0ff",
              "77.88.8.88",
              "77.88.8.2",
              "2a02:6b8::feed:bad",
              "2a02:6b8:0:1::feed:bad",
              "77.88.8.7",
              "77.88.8.3",
              "2a02:6b8::feed:a11",
              "2a02:6b8:0:1::feed:a11"
            ]
          },
          {
            "remarks": "代理海外公共DNS域名",
            "outboundTag": "proxy",
            "domain": [
              "domain:cloudflare-dns.com",
              "domain:one.one.one.one",
              "domain:dns.google",
              "domain:adguard-dns.com",
              "domain:opendns.com",
              "domain:umbrella.com",
              "domain:quad9.net",
              "domain:yandex.net"
            ]
          },
          {
            "remarks": "代理IP",
            "outboundTag": "proxy",
            "ip": [
              "geoip:facebook",
              "geoip:fastly",
              "geoip:google",
              "geoip:netflix",
              "geoip:telegram",
              "geoip:twitter"
            ]
          },
          {
            "remarks": "代理GFW",
            "outboundTag": "proxy",
            "domain": [
              "geosite:gfw",
              "geosite:greatfire"
            ]
          },
          {
            "remarks": "最终直连",
            "port": "0-65535",
            "outboundTag": "direct"
          }
        ]
        """;

    // Original upstream "V4-" template: the signature of this text is the stored OldSignature.
    private const string OldGlobal = """
        [
         {
          "remarks": "阻断udp443",
          "outboundTag": "block",
          "port": "443",
          "network": "udp"
         },
         {
          "remarks": "绕过局域网IP",
          "outboundTag": "direct",
          "ip": [
           "geoip:private"
          ]
         },
         {
          "remarks": "绕过局域网域名",
          "outboundTag": "direct",
          "domain": [
           "geosite:private"
          ]
         },
         {
          "remarks": "最终代理",
          "port": "0-65535",
          "outboundTag": "proxy"
         }
        ]
        """;

    private static string OldJson(string key) => key switch
    {
        "white" => OldWhite,
        "black" => OldBlack,
        _ => OldGlobal,
    };

    private static string NewJson(string key) => EmbedUtils.GetEmbedText(Global.CustomRoutingFileName + key);

    private static bool HasCondition(RulesItem r)
    {
        return (r.Ip?.Count > 0) || (r.Domain?.Count > 0) || (r.Protocol?.Count > 0) || (r.Process?.Count > 0)
               || !string.IsNullOrEmpty(r.Port);
    }

    private static RoutingItem Item(string remarks, string ruleSet)
    {
        return new RoutingItem { Remarks = remarks, RuleSet = ruleSet };
    }

    [Test]
    [Arguments("white")]
    [Arguments("black")]
    [Arguments("global")]
    public async Task Old_template_signature_matches_stored_constant(string key)
    {
        await ForkRoutingUpgrade.SignatureOf(OldJson(key)).Should().BeEqualTo(ForkRoutingUpgrade.OldSignatureFor(key));
    }

    [Test]
    [Arguments("white", "V4-绕过大陆(Whitelist)")]
    [Arguments("black", "V4-黑名单(Blacklist)")]
    [Arguments("global", "V4-全局(Global)")]
    public async Task Unmodified_old_item_is_matched_for_upgrade(string key, string remarks)
    {
        await ForkRoutingUpgrade.MatchUnmodifiedOld(Item(remarks, OldJson(key))).Should().BeEqualTo(key);
    }

    [Test]
    [Arguments("white", "V4-绕过大陆(Whitelist)")]
    [Arguments("black", "V4-黑名单(Blacklist)")]
    public async Task Edited_old_item_is_not_matched(string key, string remarks)
    {
        var rules = JsonUtils.Deserialize<List<RulesItem>>(OldJson(key))!;
        rules[0].Enabled = false;
        var edited = JsonUtils.Serialize(rules, false);

        await ForkRoutingUpgrade.MatchUnmodifiedOld(Item(remarks, edited)).Should().BeNull();
    }

    [Test]
    public async Task Item_with_wrong_name_or_already_upgraded_is_not_matched()
    {
        await ForkRoutingUpgrade.MatchUnmodifiedOld(Item("V4V6-绕过大陆(Whitelist)", OldWhite)).Should().BeNull();
        await ForkRoutingUpgrade.MatchUnmodifiedOld(Item("自定义(Custom)", OldWhite)).Should().BeNull();
        await ForkRoutingUpgrade.MatchUnmodifiedOld(Item("V4-绕过大陆(Whitelist)", string.Empty)).Should().BeNull();
    }

    [Test]
    [Arguments("white")]
    [Arguments("black")]
    [Arguments("global")]
    public async Task New_template_parses_and_uses_only_known_outbounds(string key)
    {
        var rules = JsonUtils.Deserialize<List<RulesItem>>(NewJson(key))!;

        await rules.Should().NotBeEmpty();
        await rules.All(r => r.OutboundTag is "direct" or "proxy" or "block").Should().BeTrue();
    }

    [Test]
    [Arguments("white")]
    [Arguments("global")]
    public async Task New_template_starts_with_private_ip_direct_rule(string key)
    {
        var first = JsonUtils.Deserialize<List<RulesItem>>(NewJson(key))![0];

        await first.OutboundTag.Should().BeEqualTo("direct");
        await first.Ip!.Should().Contain("geoip:private");
    }

    [Test]
    public async Task Black_template_keeps_bittorrent_then_private_ip_direct_first()
    {
        var rules = JsonUtils.Deserialize<List<RulesItem>>(NewJson("black"))!;

        await rules[0].Protocol!.Should().Contain("bittorrent");
        await rules[1].Ip!.Should().Contain("geoip:private");
    }

    [Test]
    public async Task White_template_has_no_catch_all_rule()
    {
        var rules = JsonUtils.Deserialize<List<RulesItem>>(NewJson("white"))!;

        await rules.Any(r => !HasCondition(r)).Should().BeFalse();
    }
}
