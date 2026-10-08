using System.Text.Json;
using ServiceLib.Handler;

namespace ServiceLib.Tests.Handler;

public class ConfigHandlerSaveConfigTests
{
    [Test]
    public async Task WriteConfigFileAsync_ParallelWrites_CompleteAndLeaveValidJson()
    {
        var dir = Path.Combine(Path.GetTempPath(), "v2rayN-save-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var resPath = Path.Combine(dir, "guiNConfig.json");
        try
        {
            var writes = Enumerable.Range(0, 50)
                .Select(i => ConfigHandler.WriteConfigFileAsync(resPath, JsonSerializer.Serialize(new { index = i, payload = new string('x', 2000) })))
                .ToArray();

            await Task.WhenAll(writes);

            var index = JsonDocument.Parse(await File.ReadAllTextAsync(resPath)).RootElement.GetProperty("index").GetInt32();
            await (index is >= 0 and < 50).Should().BeTrue();
            await File.Exists(resPath + "_temp").Should().BeFalse();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
