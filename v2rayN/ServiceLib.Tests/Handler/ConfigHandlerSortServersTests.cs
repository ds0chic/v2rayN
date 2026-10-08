using ServiceLib.Common;
using ServiceLib.Handler;
using ServiceLib.Manager;
using ServiceLib.Models.Entities;
using ServiceLib.Tests.CoreConfig;

namespace ServiceLib.Tests.Handler;

public class ConfigHandlerSortServersTests
{
    [Test]
    public async Task SortServers_ByDelay_FailedNodesSortAfterAllSuccessfulNodes()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        CoreConfigTestFactory.BindAppManagerConfig(config);
        SQLiteHelper.Instance.CreateTable<SubItem>();
        SQLiteHelper.Instance.CreateTable<ProfileItem>();
        SQLiteHelper.Instance.CreateTable<ProfileExItem>();

        var subId = $"sub-{Guid.NewGuid():N}";
        var ids = new List<string>();
        for (var i = 0; i < 4; i++)
        {
            var id = $"srv-{Guid.NewGuid():N}";
            ids.Add(id);
            await SQLiteHelper.Instance.ReplaceAsync(new ProfileItem
            {
                IndexId = id,
                Subid = subId,
                ConfigType = EConfigType.VLESS,
                Remarks = $"node-{i}",
                Address = "127.0.0.1",
                Port = 443 + i,
            });
        }

        // ids[0] = 100ms, ids[1] = failed (-1), ids[2] = 50ms, ids[3] = failed (0)
        ProfileExManager.Instance.SetTestDelay(ids[0], 100);
        ProfileExManager.Instance.SetTestDelay(ids[1], -1);
        ProfileExManager.Instance.SetTestDelay(ids[2], 50);
        ProfileExManager.Instance.SetTestDelay(ids[3], 0);

        await ConfigHandler.SortServers(config, subId, EServerColName.DelayVal.ToString(), true);

        var successMax = Math.Max(ProfileExManager.Instance.GetSort(ids[0]), ProfileExManager.Instance.GetSort(ids[2]));
        var failMin = Math.Min(ProfileExManager.Instance.GetSort(ids[1]), ProfileExManager.Instance.GetSort(ids[3]));

        await (failMin > successMax).Should().BeEqualTo(true);
    }
}
