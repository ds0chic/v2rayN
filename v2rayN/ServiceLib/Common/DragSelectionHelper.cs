namespace ServiceLib.Common;

/// <summary>
/// Pure range math for the profile list drag selection (fork only).
/// </summary>
public static class DragSelectionHelper
{
    /// <summary>
    /// Inclusive range between the anchor row and the current row, both clamped to [0, count - 1].
    /// Returns null when there are no rows.
    /// </summary>
    public static (int First, int Last)? GetRange(int anchor, int current, int count)
    {
        if (count <= 0)
        {
            return null;
        }
        var a = Math.Clamp(anchor, 0, count - 1);
        var c = Math.Clamp(current, 0, count - 1);
        return (Math.Min(a, c), Math.Max(a, c));
    }

    /// <summary>
    /// Index of the row whose band [Top, Top + Height) contains y, or -1 when y is above, below or between rows.
    /// Rows are in visual order (top to bottom) and y is in the same coordinate space as the row tops.
    /// </summary>
    public static int GetRowIndexAt(IReadOnlyList<(double Top, double Height)> rows, double y)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            var (top, height) = rows[i];
            if (y >= top && y < top + height)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// -1 when the pointer is above the rows viewport, 1 when below it, 0 when inside.
    /// </summary>
    public static int GetScrollDirection(double y, double top, double bottom)
    {
        if (y < top)
        {
            return -1;
        }
        if (y >= bottom)
        {
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// Rows to move per auto-scroll tick: 0 inside the viewport, otherwise -1 to -4 above it and 1 to 4 below it.
    /// One row near the edge, one more for every 40 px further away, capped at 4.
    /// </summary>
    public static int GetAutoScrollStep(double y, double top, double bottom)
    {
        var direction = GetScrollDirection(y, top, bottom);
        if (direction == 0)
        {
            return 0;
        }
        var distance = direction < 0 ? top - y : y - bottom;
        var rows = Math.Clamp(1 + (int)(distance / 40), 1, 4);
        return direction * rows;
    }

    /// <summary>
    /// Moves an index by step rows, clamped to [0, count - 1].
    /// </summary>
    public static int Advance(int current, int step, int count)
    {
        return Math.Clamp(current + step, 0, Math.Max(count - 1, 0));
    }
}
