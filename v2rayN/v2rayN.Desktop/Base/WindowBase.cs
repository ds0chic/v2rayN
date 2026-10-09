using v2rayN.Desktop.Common;

namespace v2rayN.Desktop.Base;

public class WindowBase<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
{
    public WindowBase()
    {
        Loaded += OnLoaded;
        Opened += (s, e) => DarkTitleBarHelper.Apply(this);
        Loaded += (s, e) =>
        {
            if (Owner != null && !ShowInTaskbar)
            {
                CanMinimize = false;
            }
        };
    }

    protected virtual void OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            var sizeItem = ConfigHandler.GetWindowSizeItem(AppManager.Instance.Config, GetType().Name);

            var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
            if (screen is null)
            {
                return;
            }
            var scaling = screen.Scaling > 0 ? screen.Scaling : 1.0;
            var workingArea = screen.WorkingArea;

            // fork: with no saved size the XAML default (e.g. 1200x800) used to be kept as is, which on a scaled laptop
            // screen (1920x1080 at 150% is 1280x720 logical) is larger than the screen. Clamp it like a saved size.
            var wantWidth = sizeItem?.Width ?? Width;
            var wantHeight = sizeItem?.Height ?? Height;
            if (double.IsNaN(wantWidth) || double.IsNaN(wantHeight))
            {
                return;
            }

            var width = Math.Min(wantWidth, workingArea.Width / scaling);
            var height = Math.Min(wantHeight, workingArea.Height / scaling);
            if (sizeItem is null && width >= wantWidth && height >= wantHeight)
            {
                return; // default already fits: keep the XAML placement (CenterOwner etc.)
            }

            Width = width;
            Height = height;

            var frameDiff = (FrameSize ?? ClientSize) - ClientSize;
            var totalWidth = (width + frameDiff.Width) * scaling;
            var totalHeight = (height + frameDiff.Height) * scaling;

            var x = workingArea.X + ((workingArea.Width - totalWidth) / 2);
            var y = workingArea.Y + ((workingArea.Height - totalHeight) / 2);
            Position = new PixelPoint((int)x, (int)y);
        }
        catch { }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        try
        {
            ConfigHandler.SaveWindowSizeItem(AppManager.Instance.Config, GetType().Name, Width, Height);
        }
        catch { }
    }
}
