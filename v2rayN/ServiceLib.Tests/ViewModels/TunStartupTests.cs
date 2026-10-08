using ServiceLib.ViewModels;

namespace ServiceLib.Tests.ViewModels;

public class TunStartupTests
{
    [Test]
    [Arguments(false, false, true, false, false)]
    [Arguments(false, false, false, false, false)]
    [Arguments(true, false, true, true, true)]   // older config: only EnableTun was saved
    [Arguments(false, true, true, true, true)]   // remembered choice, now privileged
    [Arguments(true, true, true, true, true)]
    [Arguments(true, false, false, false, true)] // not privileged: off, but remembered
    [Arguments(false, true, false, false, true)] // not privileged: off, still remembered
    public async Task ResolveTunOnStartup_FollowsLastChoice(bool enableTun, bool last, bool allowed, bool expectEnable, bool expectRemembered)
    {
        var (enable, remembered) = StatusBarViewModel.ResolveTunOnStartup(enableTun, last, allowed);

        await enable.Should().BeEqualTo(expectEnable);
        await remembered.Should().BeEqualTo(expectRemembered);
    }

    [Test]
    public async Task ResolveTunOnStartup_UnprivilegedStartThenPrivilegedStart_RestoresTun()
    {
        // Start 1 without rights: TUN off, the saved choice survives a save/load round trip.
        var first = StatusBarViewModel.ResolveTunOnStartup(enableTun: true, lastEnableTun: false, allowed: false);
        var saved = new TunModeItem { EnableTun = first.Enable, LastEnableTun = first.Remembered };
        var reloaded = JsonUtils.Deserialize<TunModeItem>(JsonUtils.Serialize(saved))!;

        // Start 2 with rights: TUN comes back.
        var second = StatusBarViewModel.ResolveTunOnStartup(reloaded.EnableTun, reloaded.LastEnableTun, allowed: true);

        await first.Enable.Should().BeFalse();
        await second.Enable.Should().BeTrue();
    }
}
