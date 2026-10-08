namespace ServiceLib.Tests.Common;

public class WindowsUtilsTests
{
    [Test]
    public async Task GetTunDeviceGuid_XrayTun_MatchesDeviceObservedOnRealMachine()
    {
        // The Wintun device left behind by Xray's "xray_tun" adapter has this instance id
        var guid = WindowsUtils.GetTunDeviceGuid("xray_tun");

        await guid.Should().BeEqualTo(Guid.Parse("7d7d9015-6c82-d838-2430-f41b93b28148"));
    }

    [Test]
    public async Task GetTunDeviceGuid_IsDeterministicAndDiffersPerName()
    {
        var a1 = WindowsUtils.GetTunDeviceGuid("xray_tun");
        var a2 = WindowsUtils.GetTunDeviceGuid("xray_tun");
        var b = WindowsUtils.GetTunDeviceGuid("wintunsingbox_tun");

        await a1.Should().BeEqualTo(a2);
        await (a1 != b).Should().BeTrue();
    }
}
