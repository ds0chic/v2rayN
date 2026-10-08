namespace ServiceLib.Helper;

/// <summary>
/// Throttles statistics UI refreshes. A traffic update that the interval gate rejects stays pending,
/// so the next idle (zero-traffic) update still applies the final totals instead of leaving a stale value.
/// </summary>
public sealed class StatisticsRefreshGate
{
    private readonly IntervalGate _gate;
    private readonly Lock _lock = new();
    private bool _pending;

    public StatisticsRefreshGate(long intervalMs)
    {
        _gate = new IntervalGate(intervalMs);
    }

    public bool ShouldRefresh(bool hasTraffic, long nowMs)
    {
        lock (_lock)
        {
            if (!hasTraffic && !_pending)
            {
                return false;
            }
            if (!_gate.TryPass(nowMs))
            {
                _pending = true;
                return false;
            }
            _pending = false;
            return true;
        }
    }
}
