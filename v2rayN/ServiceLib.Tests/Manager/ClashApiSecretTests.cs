using System.Net;
using ServiceLib.Helper;
using ServiceLib.Manager;
using ServiceLib.Models.CoreConfigs;
using ServiceLib.Services.CoreConfig;
using ServiceLib.Tests.CoreConfig;

namespace ServiceLib.Tests.Manager;

public class ClashApiSecretTests
{
    [Test]
    [NotInParallel]
    public async Task Apply_WithoutUserSecret_ReturnsGeneratedSecretAsActive()
    {
        var secret = ClashApiSecret.Apply();

        await (secret.Length >= 32).Should().BeTrue();
        await ClashApiSecret.Active.Should().BeEqualTo(secret);
        await ClashApiSecret.Apply("").Should().BeEqualTo(secret);
    }

    [Test]
    [NotInParallel]
    public async Task Apply_WithUserSecret_KeepsUserSecretActive()
    {
        var userSecret = $"user-{Guid.NewGuid():N}";
        try
        {
            await ClashApiSecret.Apply(userSecret).Should().BeEqualTo(userSecret);
            await ClashApiSecret.Active.Should().BeEqualTo(userSecret);
        }
        finally
        {
            ClashApiSecret.Apply();
        }
    }

    [Test]
    [NotInParallel]
    public async Task ClashApiHttpHelper_Request_CarriesBearerSecret()
    {
        var handler = new CapturingHandler();
        var helper = new ClashApiHttpHelper(handler);
        ClashApiSecret.Apply($"hdr-{Guid.NewGuid():N}");
        try
        {
            await helper.TryGetAsync("http://127.0.0.1:9/proxies");
            await handler.LastAuthorization.Should().BeEqualTo($"Bearer {ClashApiSecret.Active}");
        }
        finally
        {
            ClashApiSecret.Apply();
        }
    }

    [Test]
    public async Task SingboxConfig_ClashApi_CarriesActiveSecret()
    {
        var config = CoreConfigTestFactory.CreateConfig(ECoreType.sing_box);
        CoreConfigTestFactory.BindAppManagerConfig(config);
        var node = CoreConfigTestFactory.CreateSocksNode(ECoreType.sing_box);
        var context = CoreConfigTestFactory.CreateContext(config, node, ECoreType.sing_box);

        var result = new CoreConfigSingboxService(context).GenerateClientConfigContent();

        await result.Success.Should().BeTrue().Because($"ret msg: {result.Msg}");
        var cfg = JsonUtils.Deserialize<SingboxConfig>(result.Data!.ToString())!;
        // Apply() without a user secret always yields the per-run generated secret, so this comparison is stable.
        await cfg.experimental!.clash_api!.secret.Should().BeEqualTo(ClashApiSecret.Apply());
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public string? LastAuthorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastAuthorization = request.Headers.Authorization?.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
        }
    }
}
