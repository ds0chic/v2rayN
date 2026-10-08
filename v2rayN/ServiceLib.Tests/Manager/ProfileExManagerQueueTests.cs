using ServiceLib.Manager;

namespace ServiceLib.Tests.Manager;

public class ProfileExManagerQueueTests
{
    [Test]
    public async Task SetTestDelay_ConcurrentDuplicateIds_QueuesEachIdOnce()
    {
        var manager = new ProfileExManager();
        var ids = Enumerable.Range(0, 200).Select(i => $"pex-{i}-{Guid.NewGuid():N}").ToList();

        Parallel.For(0, 4000, i => manager.SetTestDelay(ids[i % ids.Count], i));

        await manager.PendingIndexIdCount.Should().BeEqualTo(ids.Count);
    }
}
