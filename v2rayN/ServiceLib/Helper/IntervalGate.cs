namespace ServiceLib.Helper;

/// <summary>
/// Lets a call through at most once per interval, measured with a monotonic millisecond clock.
/// </summary>
public sealed class IntervalGate
{
    private readonly long _intervalMs;
    private readonly Lock _lock = new();
    private bool _opened;
    private long _lastPassMs;

    public IntervalGate(long intervalMs)
    {
        _intervalMs = intervalMs;
    }

    public bool TryPass(long nowMs)
    {
        lock (_lock)
        {
            if (_opened && nowMs - _lastPassMs < _intervalMs)
            {
                return false;
            }
            _opened = true;
            _lastPassMs = nowMs;
            return true;
        }
    }
}
