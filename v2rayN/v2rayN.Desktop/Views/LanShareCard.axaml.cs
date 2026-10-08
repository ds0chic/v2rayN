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
        btnAddFirewallRule.Click += BtnAddFirewallRule_Click;
        pnlFirewall.IsVisible = Utils.IsWindows();

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
        btnAddFirewallRule.IsEnabled = _vm.AllowLANConn;
    }

    private void RefreshStatus()
    {
        var state = LanShare.GetState(_vm!.AllowLANConn, _vm.NewPort4LAN, _vm.User, _vm.Pass);
        string text;
        string tip;
        string statusClass;
        switch (state)
        {
            case LanShareState.Recommended:
                (text, tip, statusClass) = (ForkText.LanStatusRecommended, ForkText.LanTipRecommended, "lanStatusOk");
                break;
            case LanShareState.NoAuth:
                (text, tip, statusClass) = (ForkText.LanStatusNoAuth, ForkText.LanTipNoAuth, "lanStatusWarn");
                break;
            case LanShareState.SharedPort:
                (text, tip, statusClass) = (ForkText.LanStatusSharedPort, ForkText.LanTipSharedPort, "lanStatusDanger");
                break;
            default:
                (text, tip, statusClass) = (ForkText.LanStatusOff, ForkText.LanTipOff, "lanStatusMuted");
                break;
        }

        txtLanShareStatus.Text = text;
        SetStatusClass(dotLanStatus, statusClass);
        txtLanShareStatusTip.Text = tip;
        SetStatusClass(txtLanShareStatusTip, state is LanShareState.Recommended or LanShareState.Off ? "lanStatusMuted" : statusClass);

        chipLanUnsaved.IsVisible = IsUnsaved();
        txtLanUnsaved.Text = ForkText.LanUnsaved;
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
        var port = LanShare.GetLanPort(_vm.LocalPort, _vm.NewPort4LAN);
        var withAuth = LanShare.AuthApplies(allow, _vm.NewPort4LAN, _vm.User, _vm.Pass);

        _httpUrl = LanShare.BuildProxyUrl("http", ip, port, _vm.User, _vm.Pass, withAuth);
        _socksUrl = LanShare.BuildProxyUrl("socks5", ip, port, _vm.User, _vm.Pass, withAuth);

        txtLanPort.Text = port.ToString();
        txtLanHttpProxy.Text = LanShare.BuildProxyUrl("http", ip, port, _vm.User, _vm.Pass, withAuth, !_showPassword);
        txtLanSocksProxy.Text = LanShare.BuildProxyUrl("socks5", ip, port, _vm.User, _vm.Pass, withAuth, !_showPassword);

        btnToggleLanPassword.IsVisible = withAuth;
        btnToggleLanPassword.Content = _showPassword ? ForkText.LanShareHidePassword : ForkText.LanShareShowPassword;
    }

    // True when the window holds LAN values that differ from what is stored in the config.
    private bool IsUnsaved()
    {
        var saved = AppManager.Instance.Config.Inbound.FirstOrDefault();
        if (saved is null || _vm is null)
        {
            return false;
        }

        return saved.AllowLANConn != _vm.AllowLANConn
               || saved.NewPort4LAN != _vm.NewPort4LAN
               || (saved.User ?? string.Empty) != (_vm.User ?? string.Empty)
               || (saved.Pass ?? string.Empty) != (_vm.Pass ?? string.Empty);
    }

    private static void SetStatusClass(StyledElement block, string statusClass)
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
            _vm.User = LanShare.GenerateRandomUser();
        }

        if (_vm.Pass.IsNullOrEmpty())
        {
            _vm.Pass = LanShare.GenerateRandomPass();
        }

        Refresh();
    }

    private async void BtnAddFirewallRule_Click(object? sender, RoutedEventArgs e)
    {
        if (_vm is null || !Utils.IsWindows())
        {
            return;
        }

        if (!Utils.IsAdministrator())
        {
            txtFirewallResult.Text = ForkText.LanFirewallNeedAdmin;
            return;
        }

        var port = LanShare.GetLanPort(_vm.LocalPort, _vm.NewPort4LAN);
        var confirm = await UI.ShowYesNo($"{ForkText.LanFirewallConfirm}\nTCP {port}");
        if (confirm != ButtonResult.Yes)
        {
            return;
        }

        txtFirewallResult.Text = await LanShareHelper.AddFirewallRuleAsync(port);
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
