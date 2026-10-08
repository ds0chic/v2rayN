using System.ComponentModel;
using v2rayN.Desktop.Common;

namespace v2rayN.Desktop.Views;

public partial class LanShareCard : UserControl
{
    private OptionSettingViewModel? _vm;
    private List<LanShareAddress> _addresses = [];
    private string _httpUrl = string.Empty;
    private string _socksUrl = string.Empty;
    private bool _showPassword;

    private static readonly string[] LanStatusClasses = ["lanStatusOk", "lanStatusWarn", "lanStatusDanger", "lanStatusMuted"];

    public LanShareCard()
    {
        InitializeComponent();

        _addresses = LanShareHelper.GetLanIPv4Addresses();
        cmbLanIp.ItemsSource = _addresses.Select(t => $"{t.Ip}  ({t.Name})").ToList();
        cmbLanIp.SelectedIndex = _addresses.Count > 0 ? 0 : -1;
        cmbLanIp.SelectionChanged += (_, _) => Refresh();

        btnApplyLanRecommended.Click += BtnApplyLanRecommended_Click;
        btnCopyLanHttp.Click += BtnCopyLanHttp_Click;
        btnCopyLanSocks.Click += BtnCopyLanSocks_Click;
        btnToggleLanPassword.Click += (_, _) =>
        {
            _showPassword = !_showPassword;
            Refresh();
        };

        DataContextChanged += (_, _) => BindViewModel();
        AttachedToVisualTree += (_, _) => BindViewModel();
    }

    private void BindViewModel()
    {
        var vm = DataContext as OptionSettingViewModel
                 ?? (TopLevel.GetTopLevel(this) as OptionSettingWindow)?.ViewModel;
        if (ReferenceEquals(vm, _vm))
        {
            return;
        }

        if (_vm != null)
        {
            _vm.PropertyChanged -= Vm_PropertyChanged;
        }

        _vm = vm;
        if (_vm != null)
        {
            _vm.PropertyChanged += Vm_PropertyChanged;
        }

        Refresh();
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_vm is null)
        {
            return;
        }

        RefreshStatus();
        RefreshAddresses();
    }

    private void RefreshStatus()
    {
        var state = LanShareHelper.GetState(_vm!.AllowLANConn, _vm.NewPort4LAN, _vm.User, _vm.Pass);
        string text;
        string? warn;
        string statusClass;
        switch (state)
        {
            case LanShareState.Recommended:
                text = ForkText.LanStatusRecommended;
                warn = null;
                statusClass = "lanStatusOk";
                break;
            case LanShareState.NoAuth:
                text = ForkText.LanStatusNoAuth;
                warn = ForkText.LanWarnNoAuth;
                statusClass = "lanStatusWarn";
                break;
            case LanShareState.SharedPort:
                text = ForkText.LanStatusSharedPort;
                warn = ForkText.LanWarnSharedPort;
                statusClass = "lanStatusDanger";
                break;
            default:
                text = ForkText.LanStatusOff;
                warn = null;
                statusClass = "lanStatusMuted";
                break;
        }

        txtLanShareStatus.Text = text;
        SetStatusClass(txtLanShareStatus, statusClass);
        txtLanShareStatusTip.Text = warn ?? ForkText.LanStatusTip;
        SetStatusClass(txtLanShareStatusTip, warn is null ? "lanStatusMuted" : statusClass);
    }

    private void RefreshAddresses()
    {
        var allow = _vm!.AllowLANConn;
        pnlLanAddress.IsVisible = allow && _addresses.Count > 0;
        txtLanAddressHint.IsVisible = !pnlLanAddress.IsVisible;
        txtLanAddressHint.Text = !allow ? ForkText.LanShareAddressHidden : ForkText.LanShareNoIp;
        if (!pnlLanAddress.IsVisible)
        {
            return;
        }

        var index = Math.Clamp(cmbLanIp.SelectedIndex, 0, _addresses.Count - 1);
        var ip = _addresses[index].Ip;
        var port = LanShareHelper.GetLanPort(_vm.LocalPort, _vm.NewPort4LAN);
        var withAuth = LanShareHelper.AuthApplies(allow, _vm.NewPort4LAN, _vm.User, _vm.Pass);

        _httpUrl = LanShareHelper.BuildProxyUrl("http", ip, port, _vm.User, _vm.Pass, withAuth);
        _socksUrl = LanShareHelper.BuildProxyUrl("socks5", ip, port, _vm.User, _vm.Pass, withAuth);

        txtLanPort.Text = port.ToString();
        txtLanHttpProxy.Text = LanShareHelper.BuildProxyUrl("http", ip, port, _vm.User, _vm.Pass, withAuth, !_showPassword);
        txtLanSocksProxy.Text = LanShareHelper.BuildProxyUrl("socks5", ip, port, _vm.User, _vm.Pass, withAuth, !_showPassword);

        btnToggleLanPassword.IsVisible = withAuth;
        btnToggleLanPassword.Content = _showPassword ? ForkText.LanShareHidePassword : ForkText.LanShareShowPassword;
    }

    private static void SetStatusClass(TextBlock block, string statusClass)
    {
        foreach (var cls in LanStatusClasses)
        {
            block.Classes.Remove(cls);
        }

        block.Classes.Add(statusClass);
    }

    private void BtnApplyLanRecommended_Click(object? sender, RoutedEventArgs e)
    {
        if (_vm is null)
        {
            return;
        }

        _vm.AllowLANConn = true;
        _vm.NewPort4LAN = true;
        if (_vm.User.IsNullOrEmpty())
        {
            _vm.User = LanShareHelper.GenerateRandomUser();
        }

        if (_vm.Pass.IsNullOrEmpty())
        {
            _vm.Pass = LanShareHelper.GenerateRandomPass();
        }

        txtLanShareApplied.Text = ForkText.LanShareApplied;
        Refresh();
    }

    private async void BtnCopyLanHttp_Click(object? sender, RoutedEventArgs e)
    {
        await AvaUtils.SetClipboardData(this, _httpUrl);
    }

    private async void BtnCopyLanSocks_Click(object? sender, RoutedEventArgs e)
    {
        await AvaUtils.SetClipboardData(this, _socksUrl);
    }
}
