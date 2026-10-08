using System.Reflection;
using System.Runtime.CompilerServices;
using ServiceLib.Services.Statistics;

namespace ServiceLib.Tests.Services;

// Statistics values are stored and passed in KB (Xray metrics bytes / 1024, sing-box /traffic bytes / 1024);
// Utils.HumanFy takes KB. These tests lock that contract for both cores.
public class StatisticsUnitTests
{
    private const BindingFlags _flags = BindingFlags.NonPublic | BindingFlags.Instance;

    [Test]
    public async Task HumanFy_TakesKilobytes_MatchesStoredUnit()
    {
        await Utils.HumanFy(1024).Should().BeEqualTo("1.0 MB");
        await Utils.HumanFy(8).Should().BeEqualTo("8.0 KB");
        await Utils.HumanFy(431).Should().BeEqualTo("431.0 KB");
    }

    [Test]
    public async Task Xray_CumulativeBytesDelta_ConvertsToKbAndDisplays()
    {
        var svc = (StatisticsXrayService)RuntimeHelpers.GetUninitializedObject(typeof(StatisticsXrayService));
        typeof(StatisticsXrayService).GetField("_serverSpeedItem", _flags)!.SetValue(svc, new ServerSpeedItem());
        var parse = typeof(StatisticsXrayService).GetMethod("ParseOutput", _flags)!;

        var baseline = $"{{\"stats\":{{\"outbound\":{{\"{Global.ProxyTag}\":{{\"uplink\":0,\"downlink\":0}}}}}}}}";
        var after1MbUp = $"{{\"stats\":{{\"outbound\":{{\"{Global.ProxyTag}\":{{\"uplink\":1048576,\"downlink\":0}}}}}}}}";

        parse.Invoke(svc, [baseline]);
        var item = (ServerSpeedItem)parse.Invoke(svc, [after1MbUp])!;

        await item.ProxyUp.Should().BeEqualTo(1024L);
        await Utils.HumanFy(item.ProxyUp).Should().BeEqualTo("1.0 MB");
    }

    [Test]
    public async Task SingboxTraffic_BytesPerSecond_ConvertsToKbAndDisplays()
    {
        var svc = (StatisticsSingboxService)RuntimeHelpers.GetUninitializedObject(typeof(StatisticsSingboxService));
        var parse = typeof(StatisticsSingboxService).GetMethod("ParseOutput", _flags)!;

        var args = new object[] { "{\"up\":1048576,\"down\":2048}", 0UL, 0UL };
        parse.Invoke(svc, args);

        var up = (ulong)args[1];
        var down = (ulong)args[2];
        await up.Should().BeEqualTo(1048576UL);
        await down.Should().BeEqualTo(2048UL);

        // Same divisor the sing-box stats loop applies before storing.
        await Utils.HumanFy((long)(up / 1024)).Should().BeEqualTo("1.0 MB");
        await Utils.HumanFy((long)(down / 1024)).Should().BeEqualTo("2.0 KB");
    }
}
