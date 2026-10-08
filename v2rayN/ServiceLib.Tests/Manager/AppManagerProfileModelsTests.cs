using ServiceLib.Helper;
using ServiceLib.Manager;
using ServiceLib.Models.Entities;

namespace ServiceLib.Tests.Manager;

public class AppManagerProfileModelsTests
{
    [Test]
    public async Task ProfileModels_LikeWildcardsAndSubidAreMatchedLiterally()
    {
        SQLiteHelper.Instance.CreateTable<SubItem>();
        SQLiteHelper.Instance.CreateTable<ProfileItem>();

        var token = $"tk{Guid.NewGuid():N}";
        var subId = $"sub-{Guid.NewGuid():N}";
        var otherSub = $"sub-{Guid.NewGuid():N}";
        var underscoreHit = $"us-{Guid.NewGuid():N}";
        var notUnderscore = $"nu-{Guid.NewGuid():N}";
        var percentHit = $"pc-{Guid.NewGuid():N}";
        var notPercent = $"np-{Guid.NewGuid():N}";
        var quoteHit = $"qt-{Guid.NewGuid():N}";
        var otherSubHit = $"os-{Guid.NewGuid():N}";

        await Save(underscoreHit, subId, $"{token}_a");
        await Save(notUnderscore, subId, $"{token}Xa");
        await Save(percentHit, subId, $"{token}%b");
        await Save(notPercent, subId, $"{token}Zb");
        await Save(quoteHit, subId, $"{token}its");
        await Save(otherSubHit, otherSub, $"{token}_a");

        var underscore = (await AppManager.Instance.ProfileModels("", $"{token}_")).Select(t => t.IndexId).ToList();
        await underscore.Should().Contain(underscoreHit);
        await underscore.Should().Contain(otherSubHit);
        await underscore.Should().NotContain(notUnderscore);

        var underscoreInSub = (await AppManager.Instance.ProfileModels(subId, $"{token}_")).Select(t => t.IndexId).ToList();
        await string.Join(",", underscoreInSub).Should().BeEqualTo(underscoreHit);

        var percent = (await AppManager.Instance.ProfileModels(subId, $"{token}%")).Select(t => t.IndexId).ToList();
        await string.Join(",", percent).Should().BeEqualTo(percentHit);

        // Single quotes are stripped from the filter, as before: "it's" matches "its".
        var quote = (await AppManager.Instance.ProfileModels(subId, $"{token}it's")).Select(t => t.IndexId).ToList();
        await string.Join(",", quote).Should().BeEqualTo(quoteHit);
    }

    private static async Task Save(string id, string subId, string remarks)
    {
        await SQLiteHelper.Instance.ReplaceAsync(new ProfileItem
        {
            IndexId = id,
            Subid = subId,
            ConfigType = EConfigType.VLESS,
            Remarks = remarks,
            Address = "127.0.0.1",
            Port = 443,
        });
    }
}
