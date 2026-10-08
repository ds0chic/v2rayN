using System.Globalization;

namespace v2rayN.Desktop.Common;

// Fork-only UI strings. Kept out of ResUI.resx on purpose so upstream resource changes never conflict.
// Supports zh-Hans, zh-Hant and English (fallback for every other language).
public static class ForkText
{
    private static readonly string Lang = Resolve(CultureInfo.CurrentUICulture);

    public static string TabLocalPorts => Pick("本地端口与局域网", "本地連接埠與區域網路", "Local ports & LAN");
    public static string TabSniffingOutbound => Pick("流量探测与出站", "流量探測與出站", "Sniffing & outbound");
    public static string TabConnection => Pick("连接参数", "連線參數", "Connection");
    public static string TabLogCache => Pick("日志与缓存", "日誌與快取", "Logs & cache");
    public static string TabGeneral => Pick("常规与界面", "一般與介面", "General & UI");
    public static string TabSpeedSub => Pick("测速与订阅", "測速與訂閱", "Speed test & subscription");
    public static string TabRulesResources => Pick("规则与资源", "規則與資源", "Rules & resources");

    public static string MuxConcurrency => Pick("并发数 (concurrency)", "並行數 (concurrency)", "Xray Mux concurrency");
    public static string MuxXudpConcurrency => Pick("XUDP 并发数", "XUDP 並行數", "Xray Mux XUDP concurrency");
    public static string MuxXudpProxyUdp443 => Pick("XUDP 代理 UDP443", "XUDP 代理 UDP443", "Xray Mux XUDP proxy UDP443");

    public static string FragmentStateOn => Pick("已启用", "已啟用", "On");
    public static string FragmentStateOff => Pick("已关闭", "已關閉", "Off");

    public static string UwpLoopbackTip => Pick(
        "仅对 Microsoft Store/UWP 应用走本地代理有用;会打开一个小工具,需要在其中勾选应用",
        "僅對 Microsoft Store/UWP 應用走本地代理有用；會開啟一個小工具，需要在其中勾選應用",
        "Only useful for Microsoft Store/UWP apps going through the local proxy. Opens a small tool where you tick the apps.");

    public static string UwpLoopbackMissing => Pick(
        "未找到 EnableLoopback.exe,发布包才附带",
        "未找到 EnableLoopback.exe，發布包才附帶",
        "EnableLoopback.exe not found; only release packages include it.");

    public static string UwpLoopbackFailed => Pick("无法启动回环工具", "無法啟動回環工具", "Could not start the loopback tool.");

    public static string UwpLoopbackClosed => Pick("回环工具已关闭,无法确认是否有变更", "回環工具已關閉，無法確認是否有變更", "Loopback tool closed; could not confirm any change.");

    public static string UwpLoopbackUnchanged => Pick("回环豁免无变化,当前共 {0} 个应用", "回環豁免無變化，目前共 {0} 個應用", "Loopback exemptions unchanged: {0} apps.");

    public static string UwpLoopbackChanged => Pick("回环豁免已更新:共 {0} 个应用({1:+0;-0})", "回環豁免已更新：共 {0} 個應用（{1:+0;-0}）", "Loopback exemptions updated: {0} apps ({1:+0;-0}).");

    public static string ThemeDark => Pick("深色", "深色", "Dark");
    public static string ThemeLight => Pick("浅色", "淺色", "Light");

    public static string LanShareTitle =>Pick("局域网共享", "區域網路共享", "LAN sharing");

    public static string LanShareStatusLabel => Pick("当前状态", "目前狀態", "Current status");

    public static string LanStatusOff => Pick("未开启", "未開啟", "Off");

    public static string LanStatusRecommended => Pick("已开启 · 安全", "已開啟 · 安全", "On · secure");

    public static string LanStatusNoAuth => Pick("已开启 · 无认证", "已開啟 · 無認證", "On · no authentication");

    public static string LanStatusSharedPort => Pick("已开启 · 不安全", "已開啟 · 不安全", "On · insecure");

    public static string LanTipOff => Pick(
        "其他设备暂时无法使用本机代理。点右侧按钮可一键开启。",
        "其他裝置暫時無法使用本機代理。按右側按鈕可一鍵開啟。",
        "Other devices cannot use this proxy yet. Use the button on the right to turn it on.");

    public static string LanTipRecommended => Pick(
        "独立端口 + 用户名密码。其他设备按下方地址设置即可。",
        "獨立連接埠 + 使用者名稱密碼。其他裝置按下方位址設定即可。",
        "Dedicated port with a username and password. Configure other devices with the addresses below.");

    public static string LanTipNoAuth => Pick(
        "同一网络里任何设备都能使用你的代理。请填写用户名和密码。",
        "同一網路裡任何裝置都能使用你的代理。請填寫使用者名稱和密碼。",
        "Any device on the network can use your proxy. Set a username and password.");

    public static string LanTipSharedPort => Pick(
        "与本机共用端口且没有认证。点右侧按钮改用独立端口并加认证。",
        "與本機共用連接埠且沒有認證。按右側按鈕改用獨立連接埠並加認證。",
        "Shares the local port without authentication. Use the button on the right for a dedicated port with authentication.");

    public static string LanUnsaved => Pick("未保存 · 点确认生效", "未儲存 · 按確認生效", "Not saved · click OK to apply");

    public static string LanShareApplyRecommended => Pick("应用推荐设置", "套用推薦設定", "Apply recommended settings");

    public static string LanShareApplyTip => Pick(
        "开启局域网、使用独立端口并启用认证。用户名或密码为空时自动生成随机值。",
        "開啟區域網路、使用獨立連接埠並啟用認證。使用者名稱或密碼為空時自動產生隨機值。",
        "Enables LAN access on a dedicated port with authentication. An empty username or password is generated randomly.");

    public static string LanShareAddressTitle => Pick("其他设备这样设置", "其他裝置這樣設定", "Configure other devices");

    public static string LanShareAddressHidden => Pick("开启后显示", "開啟後顯示", "Shown after LAN sharing is enabled");

    public static string LanShareNoIp => Pick(
        "未检测到可用的局域网 IPv4 地址。",
        "未偵測到可用的區域網路 IPv4 位址。",
        "No usable LAN IPv4 address was detected.");

    public static string LanShareInterface => Pick("网卡", "網卡", "Network adapter");

    public static string LanShareLanPort => Pick("局域网端口", "區域網路連接埠", "LAN port");

    public static string LanShareProxyAddress => Pick("代理地址", "代理位址", "Proxy address");

    public static string LanShareCopy => Pick("复制", "複製", "Copy");

    public static string LanShareShowPassword => Pick("显示密码", "顯示密碼", "Show password");

    public static string LanShareHidePassword => Pick("隐藏密码", "隱藏密碼", "Hide password");

    public static string LanFirewallTitle => Pick("Windows 防火墙", "Windows 防火牆", "Windows Firewall");

    public static string LanFirewallButton => Pick("添加防火墙放行规则", "新增防火牆放行規則", "Add firewall allow rule");

    public static string LanFirewallTip => Pick(
        "仅放行该端口的 TCP 入站流量。可在 Windows 防火墙中删除名为 \"v2rayN LAN proxy\" 的规则。重复点击会替换同名规则。",
        "僅放行該連接埠的 TCP 入站流量。可於 Windows 防火牆中刪除名為 \"v2rayN LAN proxy\" 的規則。重複點擊會替換同名規則。",
        "Allows inbound TCP on this port only. Remove the rule named \"v2rayN LAN proxy\" in Windows Firewall to undo. Clicking again replaces the rule with the same name.");

    public static string LanFirewallConfirm => Pick(
        "将添加一条仅 TCP 的 Windows 防火墙入站放行规则,端口如下。确定继续吗?",
        "將新增一條僅 TCP 的 Windows 防火牆入站放行規則，連接埠如下。確定繼續嗎？",
        "Add an inbound Windows Firewall rule allowing TCP on the port below. Continue?");

    public static string LanFirewallNeedAdmin => Pick(
        "需要以管理员身份运行 v2rayN 才能添加防火墙规则。",
        "需要以系統管理員身分執行 v2rayN 才能新增防火牆規則。",
        "Run v2rayN as administrator to add a firewall rule.");

    public static string LanFirewallSuccess => Pick("防火墙规则已添加。", "防火牆規則已新增。", "Firewall rule added.");

    public static string LanFirewallFailed => Pick("添加防火墙规则失败。", "新增防火牆規則失敗。", "Failed to add firewall rule.");

    private static string Resolve(CultureInfo culture)
    {
        var name = culture.Name;
        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            return name.Contains("Hant", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("TW", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("HK", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("MO", StringComparison.OrdinalIgnoreCase)
                ? "zh-Hant"
                : "zh-Hans";
        }

        return "en";
    }

    private static string Pick(string hans, string hant, string en)
    {
        return Lang switch
        {
            "zh-Hans" => hans,
            "zh-Hant" => hant,
            _ => en,
        };
    }
}
