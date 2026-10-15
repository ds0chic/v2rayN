using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Path = Avalonia.Controls.Shapes.Path;
using v2rayN.Desktop.Views;
using Avalonia.Input;

namespace v2rayN.Desktop.Common;

/// <summary>
/// fork: gives every dialog window the same title bar as the main window's unified toolbar. The system border and resize
/// grips stay native (BorderOnly); the caption buttons carry Avalonia's element roles so the system treats them as such.
/// </summary>
public static class CdsChrome
{
    private const double BarHeight = 34;

    public static void Attach(Window window)
    {
        if (!OperatingSystem.IsWindows()
            || window is MainWindow
            || window.GetType().Namespace != "v2rayN.Desktop.Views"
            || window.Tag as string == "cds-chrome"
            || window.Content is not Control original)
        {
            return;
        }

        window.Tag = "cds-chrome";
        window.WindowDecorations = WindowDecorations.BorderOnly;
        window.ExtendClientAreaToDecorationsHint = true;
        window.ExtendClientAreaTitleBarHeightHint = BarHeight;

        var title = new TextBlock
        {
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            IsHitTestVisible = false,
            Margin = new Thickness(0, 0, 8, 0),
        };
        title.Bind(TextBlock.TextProperty, window.GetObservable(Window.TitleProperty));

        var buttons = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        if (window.CanResize)
        {
            var min = Caption("M0 5.5h10", WindowDecorationsElementRole.MinimizeButton);
            min.Click += (_, _) => window.WindowState = WindowState.Minimized;
            var max = Caption("M0.5 0.5h9v9h-9z", WindowDecorationsElementRole.MaximizeButton);
            var maxPath = (Path)max.Content!;
            max.Click += (_, _) => window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            window.GetObservable(Window.WindowStateProperty).Subscribe(s =>
                maxPath.Data = Geometry.Parse(s == WindowState.Maximized ? "M2.5 0.5h7v7h-2M0.5 2.5h7v7h-7z" : "M0.5 0.5h9v9h-9z"));
            buttons.Children.Add(min);
            buttons.Children.Add(max);
        }

        var close = Caption("M0 0L10 10M10 0L0 10", WindowDecorationsElementRole.CloseButton);
        close.Classes.Add("close");
        close.Click += (_, _) => window.Close();
        buttons.Children.Add(close);

        var bar = new Grid
        {
            Height = BarHeight,
            Background = Brushes.Transparent,
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
        };
        WindowDecorationProperties.SetElementRole(bar, WindowDecorationsElementRole.TitleBar);
        try
        {
            bar.Children.Add(new Image { Source = new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(new Uri("avares://v2rayN/Assets/NotifyIcon1.ico"))), Width = 14, Height = 14, Margin = new Thickness(12, 0, 8, 0), IsHitTestVisible = false });
        }
        catch
        {
            title.Margin = new Thickness(14, 0, 8, 0);
        }

        Grid.SetColumn(title, 1);
        Grid.SetColumn(buttons, 2);
        bar.Children.Add(title);
        bar.Children.Add(buttons);

        window.Content = null;
        var root = new DockPanel();
        DockPanel.SetDock(bar, Dock.Top);
        root.Children.Add(bar);
        root.Children.Add(original);
        window.Content = root;
    }

    private static Button Caption(string geometry, WindowDecorationsElementRole role)
    {
        var button = new Button
        {
            Height = BarHeight,
            Content = new Path
            {
                Width = 10,
                Height = 10,
                Data = Geometry.Parse(geometry),
                StrokeThickness = 1,
                IsHitTestVisible = false,
            },
        };
        button.Classes.Add("caption");
        ((Path)button.Content).Bind(Shape.StrokeProperty, button.GetObservable(TemplatedControl.ForegroundProperty));
        WindowDecorationProperties.SetElementRole(button, role);
        return button;
    }
}
