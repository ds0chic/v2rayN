using ServiceLib.Helper;

namespace ServiceLib.Tests.Helper;

public class StatisticsRefreshGateTests
{
    [Test]
    public async Task ShouldRefresh_IdleWithoutPendingTraffic_NeverPasses()
    {
        var gate = new StatisticsRefreshGate(2500);
        await gate.ShouldRefresh(false, 0).Should().BeEqualTo(false);
        await gate.ShouldRefresh(false, 5000).Should().BeEqualTo(false);
    }

    [Test]
    public async Task ShouldRefresh_FinalTrafficTickThenIdle_IdleTickAppliesFinalTotal()
    {
        // Upload burst: 8 KB passes, the next traffic ticks are inside the 2.5 s window,
        // then traffic stops. Before the fix the last displayed value stayed at the intermediate total.
        var gate = new StatisticsRefreshGate(2500);
        await gate.ShouldRefresh(true, 0).Should().BeEqualTo(true);
        await gate.ShouldRefresh(true, 1000).Should().BeEqualTo(false);
        await gate.ShouldRefresh(true, 2000).Should().BeEqualTo(false);
        await gate.ShouldRefresh(false, 3000).Should().BeEqualTo(true);
        await gate.ShouldRefresh(false, 4000).Should().BeEqualTo(false);
    }

    [Test]
    public async Task ShouldRefresh_PendingSurvivesRejectedIdleTicks_AppliedWhenWindowOpens()
    {
        var gate = new StatisticsRefreshGate(2500);
        await gate.ShouldRefresh(true, 0).Should().BeEqualTo(true);
        await gate.ShouldRefresh(true, 1000).Should().BeEqualTo(false);
        await gate.ShouldRefresh(false, 2000).Should().BeEqualTo(false);
        await gate.ShouldRefresh(false, 3000).Should().BeEqualTo(true);
    }

    [Test]
    public async Task ShouldRefresh_SteadyTraffic_KeepsIntervalCadence()
    {
        var gate = new StatisticsRefreshGate(2500);
        var passes = new List<long>();
        for (long ms = 0; ms <= 9000; ms += 1000)
        {
            if (gate.ShouldRefresh(true, ms))
            {
                passes.Add(ms);
            }
        }

        await string.Join(",", passes).Should().BeEqualTo("0,3000,6000,9000");
    }
}
