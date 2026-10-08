using ServiceLib.Manager;
using ServiceLib.Models.Entities;
using ServiceLib.Services.CoreConfig;

namespace ServiceLib.Tests.CoreConfig.Clash;

public class CoreConfigClashSecretTests
{
    [Test]
    [NotInParallel]
    public async Task GenerateClientCustomConfig_UserSecret_IsPreservedAndActive()
    {
        var userSecret = $"user-{Guid.NewGuid():N}";
        var output = await GenerateFor($"mixed-port: 7890\nsecret: {userSecret}\nproxies: []\n");
        try
        {
            await output.Should().Contain(userSecret);
            await ClashApiSecret.Active.Should().BeEqualTo(userSecret);
        }
        finally
        {
            ClashApiSecret.Apply();
        }
    }

    [Test]
    [NotInParallel]
    public async Task GenerateClientCustomConfig_NoUserSecret_InjectsActiveSecret()
    {
        var output = await GenerateFor("mixed-port: 7890\nproxies: []\n");

        await output.Should().Contain(ClashApiSecret.Active);
        await (ClashApiSecret.Active.Length >= 32).Should().BeTrue();
    }

    private static async Task<string> GenerateFor(string yaml)
    {
        var config = CoreConfigTestFactory.CreateConfig(ECoreType.mihomo);
        CoreConfigTestFactory.BindAppManagerConfig(config);
        var source = Path.GetTempFileName();
        var output = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(source, yaml);
            var node = new ProfileItem { Address = source, ConfigType = EConfigType.Custom, Remarks = "clash-secret" };
            var ret = await new CoreConfigClashService(config, false).GenerateClientCustomConfig(node, output);
            await ret.Success.Should().BeTrue().Because($"ret msg: {ret.Msg}");
            return await File.ReadAllTextAsync(output);
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }
}
