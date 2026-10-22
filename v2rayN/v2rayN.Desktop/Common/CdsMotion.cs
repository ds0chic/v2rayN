using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace v2rayN.Desktop.Common;

/// <summary>
/// fork: pieces of the Claude design language that a style cannot express: a thumb that slides to the selected
/// segment, and scroll-edge fade masks.
/// </summary>
public static class CdsMotion
{
    /// <summary>
    /// Keeps <paramref name="thumb"/> (a sibling of <paramref name="list"/> in the same Grid, drawn behind it) on the
    /// selected item. The thumb's Margin/Width/Height carry transitions (see ForkTheme), so it slides.
    /// </summary>
    public static void AttachSegmentedThumb(ListBox list, Border thumb)
    {
        var placed = false;
        var pending = false;

        void Place()
        {
            pending = false;
            if (thumb.Parent is not Visual host
                || list.SelectedIndex < 0
                || list.ContainerFromIndex(list.SelectedIndex) is not Control item
                || item.Bounds.Width <= 0)
            {
                thumb.IsVisible = false;
                placed = false;
                return;
            }

            var p = item.TranslatePoint(new Point(0, 0), host);
            if (p is null)
            {
                return;
            }

            var margin = new Thickness(p.Value.X, p.Value.Y, 0, 0);
            if (!placed)
            {
                // first placement (or after a re-layout from nothing): jump, do not fly in from the corner
                var transitions = thumb.Transitions;
                thumb.Transitions = null;
                thumb.Width = item.Bounds.Width;
                thumb.Height = item.Bounds.Height;
                thumb.Margin = margin;
                thumb.IsVisible = true;
                placed = true;
                Dispatcher.UIThread.Post(() => thumb.Transitions = transitions, DispatcherPriority.Background);
                return;
            }

            thumb.IsVisible = true;
            thumb.Width = item.Bounds.Width;
            thumb.Height = item.Bounds.Height;
            thumb.Margin = margin;
        }

        void Queue()
        {
            if (pending)
            {
                return;
            }

            pending = true;
            Dispatcher.UIThread.Post(Place, DispatcherPriority.Loaded);
        }

        list.SelectionChanged += (_, _) => Queue();
        list.LayoutUpdated += (_, _) => Queue();
        list.AttachedToVisualTree += (_, _) => Queue();
    }

    /// <summary>
    /// Fades the top / bottom 28 px of <paramref name="target"/> to transparent, but only on the side that still has
    /// content to scroll to. <paramref name="read"/> returns (offset, maxOffset).
    /// </summary>
    private static void ApplyFadeMask(Control target, double offset, double max)
    {
        var h = target.Bounds.Height;
        if (h < 80 || max <= 1)
        {
            target.OpacityMask = null;
            return;
        }

        var f = 28.0 / h;
        var top = offset > 1;
        var bottom = offset < max - 1;
        if (!top && !bottom)
        {
            target.OpacityMask = null;
            return;
        }

        // open-claude-ui 7-stop perceptual mask over the 28 px fade (cds-theme.css, --cds-scroll-fade-size)
        double[] pos = { 0, 0.15, 0.3, 0.45, 0.6, 0.75, 0.87, 1 };
        double[] alpha = { 0, 0.05, 0.15, 0.3, 0.5, 0.7, 0.85, 1 };
        var brush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        };
        for (var i = 0; i < pos.Length; i++)
        {
            brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)((top ? alpha[i] : 1) * 255), 0, 0, 0), pos[i] * f));
        }

        for (var i = 0; i < pos.Length; i++)
        {
            var j = pos.Length - 1 - i;
            brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)((bottom ? alpha[j] : 1) * 255), 0, 0, 0), 1 - pos[j] * f));
        }

        target.OpacityMask = brush;
    }

    public static void AttachScrollFade(ScrollViewer sv, Control target)
    {
        void Update() => ApplyFadeMask(target, sv.Offset.Y, Math.Max(0, sv.Extent.Height - sv.Viewport.Height));
        sv.ScrollChanged += (_, _) => Update();
        target.SizeChanged += (_, _) => Update();
    }

    public static void AttachScrollFade(DataGrid grid)
    {
        ScrollBar? bar = null;
        Control? rows = null;

        void Update()
        {
            if (bar is null || rows is null)
            {
                return;
            }

            ApplyFadeMask(rows, bar.Value, bar.Maximum);
        }

        void Hook()
        {
            bar = grid.GetVisualDescendants().OfType<ScrollBar>().FirstOrDefault(b => b.Orientation == Avalonia.Layout.Orientation.Vertical);
            rows = grid.GetVisualDescendants().OfType<DataGridRowsPresenter>().FirstOrDefault();
            if (bar is null || rows is null)
            {
                return;
            }

            bar.GetObservable(RangeBase.ValueProperty).Subscribe(_ => Update());
            bar.GetObservable(RangeBase.MaximumProperty).Subscribe(_ => Update());
            rows.SizeChanged += (_, _) => Update();
            Update();
        }

        grid.Loaded += (_, _) => Dispatcher.UIThread.Post(Hook, DispatcherPriority.Loaded);
    }
}
