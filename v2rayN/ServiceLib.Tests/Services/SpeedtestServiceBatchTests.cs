using ServiceLib.Models.Dto;
using ServiceLib.Services;

namespace ServiceLib.Tests.Services;

public class SpeedtestServiceBatchTests
{
    private static ServerTestItem Item(string id, ECoreType coreType)
    {
        return new ServerTestItem { IndexId = id, CoreType = coreType, Address = "127.0.0.1", Port = 443 };
    }

    private static List<ServerTestItem> Mixed()
    {
        return [Item("xray", ECoreType.Xray), Item("singbox", ECoreType.sing_box), Item("hy2", ECoreType.hysteria2)];
    }

    [Test]
    public async Task GetTestBatchItem_CoreBased_ExcludesUnsupportedCores()
    {
        var batches = SpeedtestService.GetTestBatchItem(Mixed(), 10);

        await batches.Sum(b => b.Count).Should().BeEqualTo(2);
        await batches.SelectMany(b => b).Any(t => t.IndexId == "hy2").Should().BeEqualTo(false);
    }

    [Test]
    public async Task GetTestBatchItem_AllCores_IncludesEveryNode()
    {
        var batches = SpeedtestService.GetTestBatchItem(Mixed(), 10, true);

        await batches.Sum(b => b.Count).Should().BeEqualTo(3);
        await batches.SelectMany(b => b).Any(t => t.IndexId == "hy2").Should().BeEqualTo(true);
    }

    [Test]
    public async Task IsCoreSupported_OnlyXrayAndSingBox()
    {
        await SpeedtestService.IsCoreSupported(ECoreType.Xray).Should().BeEqualTo(true);
        await SpeedtestService.IsCoreSupported(ECoreType.sing_box).Should().BeEqualTo(true);
        await SpeedtestService.IsCoreSupported(ECoreType.hysteria2).Should().BeEqualTo(false);
    }

    [Test]
    public async Task PickPreferredAddress_PrefersIPv4OverIPv6()
    {
        var picked = SpeedtestService.PickPreferredAddress([IPAddress.IPv6Loopback, IPAddress.Loopback]);

        await picked.Should().BeEqualTo(IPAddress.Loopback);
    }

    [Test]
    public async Task PickPreferredAddress_NoIPv4_FallsBackToFirst()
    {
        var picked = SpeedtestService.PickPreferredAddress([IPAddress.IPv6Loopback]);

        await picked.Should().BeEqualTo(IPAddress.IPv6Loopback);
    }

    [Test]
    public async Task ResolveIPAddressAsync_UnresolvableHost_ReturnsNull()
    {
        var resolved = await SpeedtestService.ResolveIPAddressAsync("nonexistent.invalid", CancellationToken.None);

        await resolved.Should().BeEqualTo(null);
    }
}
