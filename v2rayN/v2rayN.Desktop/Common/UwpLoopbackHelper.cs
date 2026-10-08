using System.Diagnostics;
using System.Text.RegularExpressions;

namespace v2rayN.Desktop.Common;

// EnableLoopback.exe is a separate GUI tool and reports nothing back, so the result is judged by comparing the
// number of loopback-exempt apps (CheckNetIsolation) before and after the tool was used.
internal static partial class UwpLoopbackHelper
{
    [GeneratedRegex(@"^\s*\[\d+\]", RegexOptions.Multiline)]
    private static partial Regex EntryRegex();

    public static int? CountExempt()
    {
        try
        {
            using var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "CheckNetIsolation.exe",
                Arguments = "LoopbackExempt -s",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            });
            if (proc is null)
            {
                return null;
            }

            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(5000);
            return EntryRegex().Matches(output).Count;
        }
        catch
        {
            return null;
        }
    }

    // Starts the tool (it asks for elevation itself) and waits until its window is closed.
    public static async Task<bool> RunToolAsync(string path)
    {
        try
        {
            using var proc = Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            if (proc is null)
            {
                return false;
            }

            await proc.WaitForExitAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
