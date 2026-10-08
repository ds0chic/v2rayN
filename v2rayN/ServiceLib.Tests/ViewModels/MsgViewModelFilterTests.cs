using System.Text.RegularExpressions;
using ServiceLib.ViewModels;

namespace ServiceLib.Tests.ViewModels;

public class MsgViewModelFilterTests
{
    [Test]
    public async Task TryCompileMsgFilter_ValidPattern_CompilesAndMatches()
    {
        var ok = MsgViewModel.TryCompileMsgFilter(@"error\d+", out var regex, out var error);

        await ok.Should().BeTrue();
        await error.Should().BeEqualTo(string.Empty);
        await regex!.IsMatch("request error42").Should().BeTrue();
        await regex.IsMatch("all good").Should().BeFalse();
    }

    [Test]
    public async Task TryCompileMsgFilter_InvalidPattern_ReturnsFalseWithoutThrowing()
    {
        var ok = MsgViewModel.TryCompileMsgFilter("([unclosed", out var regex, out var error);

        await ok.Should().BeFalse();
        await regex.Should().BeNull();
        await error.Should().NotBeEmpty();
    }
}
