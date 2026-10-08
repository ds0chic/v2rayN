using ServiceLib.Manager;

namespace ServiceLib.Tests.Manager;

public class PacManagerTests
{
    [Test]
    public async Task ListenLoop_AbruptClientAndSilentClient_StillServesPac()
    {
        var content = Encoding.UTF8.GetBytes("HTTP/1.0 200 OK\r\n\r\nFUNCTION FindProxyForURL(url, host) { return \"DIRECT\"; }");
        var pac = new PacManager();
        pac.SetContent(content);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        using var cts = new CancellationTokenSource();
        var loop = pac.ListenLoopAsync(listener, cts.Token);
        try
        {
            // Client that connects and is reset immediately.
            using (var dropped = new TcpClient())
            {
                await dropped.ConnectAsync(IPAddress.Loopback, port);
                dropped.LingerState = new LingerOption(true, 0);
            }

            // Client that connects and sends nothing; it stays open while the next request is served.
            using var silent = new TcpClient();
            await silent.ConnectAsync(IPAddress.Loopback, port);

            using var normal = new TcpClient();
            await normal.ConnectAsync(IPAddress.Loopback, port);
            var stream = normal.GetStream();
            await stream.WriteAsync("GET /pac HTTP/1.0\r\n\r\n"u8.ToArray());

            using var readCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var received = new MemoryStream();
            await stream.CopyToAsync(received, readCts.Token);

            await Encoding.UTF8.GetString(received.ToArray()).Should().BeEqualTo(Encoding.UTF8.GetString(content));
        }
        finally
        {
            cts.Cancel();
            await loop;
        }
    }
}
