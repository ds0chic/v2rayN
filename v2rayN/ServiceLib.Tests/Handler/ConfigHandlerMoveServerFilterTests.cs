using ServiceLib.Enums;
using ServiceLib.Handler;
using ServiceLib.Manager;
using ServiceLib.Tests.CoreConfig;

namespace ServiceLib.Tests.Handler;

public class ConfigHandlerMoveServerFilterTests
{
    [Test]
    public async Task MoveServer_FilteredList_MoveUp_KeepsHiddenSortValueAndSwapsVisibleSlots()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        var visibleA = $"mvu-a-{Guid.NewGuid():N}";
        var visibleB = $"mvu-b-{Guid.NewGuid():N}";
        var hidden = $"mvu-h-{Guid.NewGuid():N}";
        ProfileExManager.Instance.SetSort(visibleA, 10);
        ProfileExManager.Instance.SetSort(hidden, 20);
        ProfileExManager.Instance.SetSort(visibleB, 30);

        // Filtered (visible) list is [A, B]; move B up above A. The hidden server stays at 20.
        await ConfigHandler.MoveServer(config, [visibleA, visibleB], 1, EMove.Up);

        await ProfileExManager.Instance.GetSort(hidden).Should().BeEqualTo(20);
        await (ProfileExManager.Instance.GetSort(visibleB) < ProfileExManager.Instance.GetSort(visibleA)).Should().BeTrue();
        await (ProfileExManager.Instance.GetSort(visibleA) + ProfileExManager.Instance.GetSort(visibleB)).Should().BeEqualTo(40);
    }

    [Test]
    public async Task MoveServer_FilteredList_MoveDown_KeepsHiddenSortValueAndSwapsVisibleSlots()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        var visibleA = $"mvd-a-{Guid.NewGuid():N}";
        var visibleB = $"mvd-b-{Guid.NewGuid():N}";
        var hidden = $"mvd-h-{Guid.NewGuid():N}";
        ProfileExManager.Instance.SetSort(visibleA, 10);
        ProfileExManager.Instance.SetSort(hidden, 20);
        ProfileExManager.Instance.SetSort(visibleB, 30);

        // Filtered (visible) list is [A, B]; move A down below B. The hidden server stays at 20.
        await ConfigHandler.MoveServer(config, [visibleA, visibleB], 0, EMove.Down);

        await ProfileExManager.Instance.GetSort(hidden).Should().BeEqualTo(20);
        await (ProfileExManager.Instance.GetSort(visibleB) < ProfileExManager.Instance.GetSort(visibleA)).Should().BeTrue();
        await (ProfileExManager.Instance.GetSort(visibleA) + ProfileExManager.Instance.GetSort(visibleB)).Should().BeEqualTo(40);
    }
}
