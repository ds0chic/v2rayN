namespace ServiceLib.Tests.Common;

public class LanShareTests
{
    [Test]
    public async Task GetLanPort_SharedPort_ReturnsLocalPort()
    {
        await LanShare.GetLanPort(10808, false).Should().BeEqualTo(10808);
    }

    [Test]
    public async Task GetLanPort_DedicatedPort_AddsSocks3Offset()
    {
        // Matches V2rayInboundService.GenInbounds: the LAN inbound sits at localPort + 2.
        await LanShare.GetLanPort(10808, true).Should().BeEqualTo(10810);
    }

    [Test]
    public async Task GetState_AllowLanOff_IsOffEvenWithCredentials()
    {
        await LanShare.GetState(false, true, "user", "pass").Should().BeEqualTo(LanShareState.Off);
        await LanShare.GetState(false, false, null, null).Should().BeEqualTo(LanShareState.Off);
    }

    [Test]
    public async Task GetState_SharedPort_WhenNotDedicated()
    {
        await LanShare.GetState(true, false, "user", "pass").Should().BeEqualTo(LanShareState.SharedPort);
        await LanShare.GetState(true, false, null, null).Should().BeEqualTo(LanShareState.SharedPort);
    }

    [Test]
    public async Task GetState_DedicatedPortWithCredentials_IsRecommended()
    {
        await LanShare.GetState(true, true, "user", "pass").Should().BeEqualTo(LanShareState.Recommended);
    }

    [Test]
    public async Task GetState_DedicatedPortWithoutCredentials_IsNoAuth()
    {
        await LanShare.GetState(true, true, "user", "").Should().BeEqualTo(LanShareState.NoAuth);
        await LanShare.GetState(true, true, null, null).Should().BeEqualTo(LanShareState.NoAuth);
    }

    [Test]
    public async Task AuthApplies_OnlyOnDedicatedPortWithLanEnabled()
    {
        await LanShare.AuthApplies(true, true, "user", "pass").Should().BeTrue();
        await LanShare.AuthApplies(true, false, "user", "pass").Should().BeFalse();
        await LanShare.AuthApplies(false, true, "user", "pass").Should().BeFalse();
    }

    [Test]
    public async Task AuthApplies_RequiresBothUserAndPass()
    {
        await LanShare.AuthApplies(true, true, "", "pass").Should().BeFalse();
        await LanShare.AuthApplies(true, true, "user", null).Should().BeFalse();
    }

    [Test]
    public async Task BuildProxyUrl_WithoutAuth_OmitsCredentials()
    {
        var url = LanShare.BuildProxyUrl("socks5", "192.168.1.10", 10810, "user", "pass", false);

        await url.Should().BeEqualTo("socks5://192.168.1.10:10810");
    }

    [Test]
    public async Task BuildProxyUrl_WithAuth_EscapesCredentials()
    {
        var url = LanShare.BuildProxyUrl("http", "192.168.1.10", 10810, "user1", "p@ss", true);

        await url.Should().BeEqualTo("http://user1:p%40ss@192.168.1.10:10810");
    }

    [Test]
    public async Task BuildProxyUrl_MaskPassword_HidesPassword()
    {
        var url = LanShare.BuildProxyUrl("http", "192.168.1.10", 10810, "user1", "p@ss", true, true);

        await url.Should().BeEqualTo("http://user1:******@192.168.1.10:10810");
    }

    [Test]
    public async Task GenerateRandomUser_IsEightAlphanumericChars()
    {
        var user = LanShare.GenerateRandomUser();

        await user.Length.Should().BeEqualTo(8);
        await user.All(char.IsAsciiLetterOrDigit).Should().BeTrue();
    }

    [Test]
    public async Task GenerateRandomPass_IsSixteenAlphanumericChars()
    {
        var pass = LanShare.GenerateRandomPass();

        await pass.Length.Should().BeEqualTo(16);
        await pass.All(char.IsAsciiLetterOrDigit).Should().BeTrue();
    }

    [Test]
    public async Task GenerateRandomCredentials_DifferBetweenCalls()
    {
        var user1 = LanShare.GenerateRandomUser();
        var user2 = LanShare.GenerateRandomUser();
        var pass1 = LanShare.GenerateRandomPass();
        var pass2 = LanShare.GenerateRandomPass();

        await (user1 != user2).Should().BeTrue();
        await (pass1 != pass2).Should().BeTrue();
    }

    [Test]
    public async Task FirewallAddArgs_LimitedToLocalSubnetPrivateAndDomainProfiles()
    {
        var args = string.Join(' ', LanShare.FirewallAddArgs(10808));

        await args.Should().BeEqualTo("advfirewall firewall add rule name=v2rayN LAN proxy dir=in action=allow protocol=TCP localport=10808 remoteip=localsubnet profile=private,domain");
    }
}
