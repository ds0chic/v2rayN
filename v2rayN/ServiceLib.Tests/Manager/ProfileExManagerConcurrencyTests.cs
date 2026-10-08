using ServiceLib.Manager;

namespace ServiceLib.Tests.Manager;

public class ProfileExManagerConcurrencyTests
{
    [Test]
    public async Task SetTestDelay_ConcurrentCallers_CreateExactlyOneItemPerIndexId()
    {
        var manager = new ProfileExManager();
        var ids = Enumerable.Range(0, 100).Select(i => $"pexc-{i}-{Guid.NewGuid():N}").ToList();

        Parallel.For(0, 2000, i => manager.SetTestDelay(ids[i % ids.Count], i));

        var items = (await manager.GetProfileExs()).Where(t => ids.Contains(t.IndexId)).ToList();
        await items.Count.Should().BeEqualTo(ids.Count);
        await items.Select(t => t.IndexId).Distinct().Count().Should().BeEqualTo(ids.Count);
    }
}
