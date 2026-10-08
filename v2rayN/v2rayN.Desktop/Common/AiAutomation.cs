using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Nodes;

namespace v2rayN.Desktop.Common;

// Opt-in UI automation pipe for AI/test driving. Enabled only when V2RAYN_AI_AUTOMATION=1.
// Local named pipe only (no network listener); access is limited to the current Windows user.
// Protocol: one command per line (UTF-8), one JSON object per response line.
public static class AiAutomation
{
    private const string EnvVarName = "V2RAYN_AI_AUTOMATION";
    private static int _started;

    public static string PipeName => $"v2rayN-ai-{Environment.ProcessId}";

    public static void Start()
    {
        if (Environment.GetEnvironmentVariable(EnvVarName) != "1")
        {
            return;
        }

        if (Design.IsDesignMode || !OperatingSystem.IsWindows())
        {
            return;
        }

        if (Interlocked.Exchange(ref _started, 1) == 1)
        {
            return;
        }

        var thread = new Thread(ServeForever) { IsBackground = true, Name = "AiAutomationPipe" };
        thread.Start();
    }

    [SupportedOSPlatform("windows")]
    private static void ServeForever()
    {
        while (true)
        {
            try
            {
                using var server = CreateServerPipe(PipeName);
                server.WaitForConnection();
                HandleClientAsync(server).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.SaveLog("AiAutomation", ex);
                Thread.Sleep(500);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private static NamedPipeServerStream CreateServerPipe(string pipeName)
    {
        using var identity = WindowsIdentity.GetCurrent();
        var security = new PipeSecurity();
        security.AddAccessRule(new PipeAccessRule(identity.User!, PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance, AccessControlType.Allow));
        // Same-user SMB/network logons must not reach the pipe.
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkSid, null), PipeAccessRights.ReadWrite, AccessControlType.Deny));

        return NamedPipeServerStreamAcl.Create(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, security);
    }

    [SupportedOSPlatform("windows")]
    private static async Task HandleClientAsync(NamedPipeServerStream server)
    {
        var utf8 = new UTF8Encoding(false);
        using var reader = new StreamReader(server, utf8, false, 4096, leaveOpen: true);
        using var writer = new StreamWriter(server, utf8, 4096, leaveOpen: true) { AutoFlush = true, NewLine = "\n" };

        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var (response, exitAfterReply) = await AiAutomationCommands.ExecuteAsync(line);
            await writer.WriteLineAsync(response.ToJsonString());

            if (exitAfterReply)
            {
                server.WaitForPipeDrain();
                Environment.Exit(0);
            }
        }
    }
}
