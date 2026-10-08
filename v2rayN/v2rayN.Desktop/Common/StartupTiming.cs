using System.Diagnostics;

namespace v2rayN.Desktop.Common;

// Temporary startup timing. Remove this file and its Mark/Flush call sites (Program.cs, App.axaml.cs) when no longer needed.
internal static class StartupTiming
{
    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private static readonly List<string> _marks = [];

    public static void Mark(string name)
    {
        _marks.Add($"{name}={_stopwatch.ElapsedMilliseconds}ms");
    }

    public static void Flush()
    {
        Logging.SaveLog($"Startup timing: {string.Join(", ", _marks)}");
    }
}
