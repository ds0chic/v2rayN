using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using AvaloniaEdit;
using Semi.Avalonia;

namespace v2rayN.Desktop.ViewModels;

public partial class ThemeSettingViewModel : MyReactiveObject
{
    [Reactive] public partial string CurrentTheme { get; set; }

    [Reactive] public partial int CurrentFontSize { get; set; }

    [Reactive] public partial string CurrentLanguage { get; set; }

    public ThemeSettingViewModel()
    {
        _config = AppManager.Instance.Config;

        BindingUI();
        RestoreUI();
    }

    private void RestoreUI()
    {
        ModifyTheme();
        ModifyFontFamily();
        ModifyFontSize();
    }

    private void BindingUI()
    {
        CurrentTheme = NormalizeTheme(_config.UiItem.CurrentTheme);
        CurrentFontSize = _config.UiItem.CurrentFontSize;
        CurrentLanguage = _config.UiItem.CurrentLanguage;

        this.WhenAnyValue(x => x.CurrentTheme)
            .Where(y => y.IsNotEmpty())
            .SubscribeAsync(async _ =>
            {
                if (_config.UiItem.CurrentTheme != CurrentTheme)
                {
                    _config.UiItem.CurrentTheme = CurrentTheme;
                    ModifyTheme();
                    await ConfigHandler.SaveConfig(_config);
                }
            });

        this.WhenAnyValue(x => x.CurrentFontSize)
            .Where(y => y > 0)
            .SubscribeAsync(async _ =>
            {
                if (_config.UiItem.CurrentFontSize != CurrentFontSize && CurrentFontSize >= Global.MinFontSize)
                {
                    _config.UiItem.CurrentFontSize = CurrentFontSize;
                    ModifyFontSize();
                    await ConfigHandler.SaveConfig(_config);
                }
            });

        this.WhenAnyValue(x => x.CurrentLanguage)
            .Where(y => !y.IsNullOrEmpty())
            .SubscribeAsync(async _ =>
            {
                if (CurrentLanguage.IsNotEmpty() && _config.UiItem.CurrentLanguage != CurrentLanguage)
                {
                    _config.UiItem.CurrentLanguage = CurrentLanguage;
                    Thread.CurrentThread.CurrentUICulture = new(CurrentLanguage);
                    await ConfigHandler.SaveConfig(_config);
                    NoticeManager.Instance.Enqueue(ResUI.NeedRebootTips);
                }
            });
    }

    // Only Dark and Light are offered; anything else (follow system, the colored variants) maps to the closest of the two.
    private static string NormalizeTheme(string? theme)
    {
        if (theme is nameof(ETheme.Dark) or nameof(ETheme.Light))
        {
            return theme;
        }

        return Application.Current?.ActualThemeVariant == ThemeVariant.Light ? nameof(ETheme.Light) : nameof(ETheme.Dark);
    }

    private void ModifyTheme()
    {
        var app = Application.Current;
        if (app is not null)
        {
            app.RequestedThemeVariant = CurrentTheme switch
            {
                nameof(ETheme.Dark) => ThemeVariant.Dark,
                nameof(ETheme.Light) => ThemeVariant.Light,
                nameof(ETheme.Aquatic) => SemiTheme.Aquatic,
                nameof(ETheme.Desert) => SemiTheme.Desert,
                nameof(ETheme.Dusk) => SemiTheme.Dusk,
                nameof(ETheme.NightSky) => SemiTheme.NightSky,
                _ => ThemeVariant.Default,
            };
        }
    }

    private void ModifyFontSize()
    {
        double size = CurrentFontSize;
        if (size < Global.MinFontSize)
        {
            return;
        }

        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        // fork: the size styles live in ForkTheme.axaml under the `userFont` window class and read their values from
        // these resources. Adding or replacing styles on Application.Styles at runtime (the old behavior) made the
        // drawn caption buttons jump left and shrink for the rest of the session and, with cached views in a
        // TabControl, crashed the app. A resource change and a class on the windows touch neither the style list.
        app.Resources["ForkUserFontSize"] = size;
        app.Resources["ForkUserRowHeight"] = 20 + (size / 2);
        _userFontActive = true;

        if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            foreach (var window in lifetime.Windows)
            {
                ApplyUserFontClass(window);
            }
        }
    }

    private static bool _userFontActive;

    /// <summary>Marks a window so the user's font size styles apply to it. Called for every window when it opens.</summary>
    public static void ApplyUserFontClass(Window window)
    {
        if (_userFontActive)
        {
            window.Classes.Set("userFont", true);
        }
    }

    private void ModifyFontFamily()
    {
        var currentFontFamily = _config.UiItem.CurrentFontFamily;
        if (currentFontFamily.IsNullOrEmpty())
        {
            return;
        }

        try
        {
            Style style = new(x => Selectors.Or(
                x.OfType<Button>(),
                x.OfType<TextBox>(),
                x.OfType<TextBlock>(),
                x.OfType<SelectableTextBlock>(),
                x.OfType<Menu>(),
                x.OfType<ContextMenu>(),
                x.OfType<DataGridRow>(),
                x.OfType<ListBoxItem>(),
                x.OfType<HeaderedContentControl>(),
                x.OfType<WindowNotificationManager>(),
                x.OfType<TextEditor>()
            ));
            style.Add(new Setter()
            {
                Property = TemplatedControl.FontFamilyProperty,
                Value = new FontFamily(currentFontFamily),
            });
            Application.Current?.Styles.Add(style);
        }
        catch (Exception ex)
        {
            Logging.SaveLog("ModifyFontFamily", ex);
        }
    }
}
