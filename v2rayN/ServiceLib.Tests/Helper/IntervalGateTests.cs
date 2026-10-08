using ServiceLib.Helper;

namespace ServiceLib.Tests.Helper;

public class IntervalGateTests
{
    [Test]
    public async Task TryPass_OneSecondTicks_PassesEveryThirdTickAtThreeSecondCadence()
    {
        var gate = new IntervalGate(2500);
        var passes = new List<long>();
        for (long ms = 0; ms <= 9000; ms += 1000)
        {
            if (gate.TryPass(ms))
            {
                passes.Add(ms);
            }
        }

        await string.Join(",", passes).Should().BeEqualTo("0,3000,6000,9000");
    }

    [Test]
    public async Task TryPass_FirstCallAlwaysPasses()
    {
        var gate = new IntervalGate(2500);
        await gate.TryPass(0).Should().BeTrue();
        await gate.TryPass(100).Should().BeEqualTo(false);
    }
}
