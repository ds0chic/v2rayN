using Avalonia.Data;
using v2rayN.Desktop.Common;
using v2rayN.Desktop.ViewModels;

namespace v2rayN.Desktop.Views;

/// <summary>
/// ThemeSettingView.xaml
/// </summary>
internal sealed record ThemeOption(string Value, string Display);

public partial class ThemeSettingView : ReactiveUserControl<ThemeSettingViewModel>
{
    public ThemeSettingView()
    {
        InitializeComponent();
        ViewModel = new ThemeSettingViewModel();

        cmbCurrentTheme.ItemsSource = new List<ThemeOption>
        {
            new(nameof(ETheme.Dark), ForkText.ThemeDark),
            new(nameof(ETheme.Light), ForkText.ThemeLight),
        };
        cmbCurrentTheme.DisplayMemberBinding = new Binding(nameof(ThemeOption.Display));
        cmbCurrentTheme.SelectedValueBinding = new Binding(nameof(ThemeOption.Value));
        cmbCurrentFontSize.ItemsSource = new List<int> { 11, 12, 13 };
        cmbCurrentLanguage.ItemsSource = Global.LanguageOptions;
        cmbCurrentLanguage.DisplayMemberBinding = new Binding(nameof(LanguageOption.Display));
        cmbCurrentLanguage.SelectedValueBinding = new Binding(nameof(LanguageOption.Value));

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.CurrentTheme, v => v.cmbCurrentTheme.SelectedValue).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.CurrentFontSize, v => v.cmbCurrentFontSize.SelectedValue).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.CurrentLanguage, v => v.cmbCurrentLanguage.SelectedValue).DisposeWith(disposables);
        });
    }
}
