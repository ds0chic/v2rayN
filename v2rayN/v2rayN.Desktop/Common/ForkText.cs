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

    public static string LanShareTitle => Pick("局域网共享", "區域網路共享", "LAN sharing");

    public static string LanShareStatusLabel => Pick("当前状态", "目前狀態", "Current status");

    public static string LanStatusOff => Pick("未开启", "未開啟", "Off");

    public static string LanStatusRecommended => Pick(
        "已开启(推荐:独立端口+认证)",
        "已開啟(推薦：獨立連接埠＋認證)",
        "On (recommended: dedicated port + auth)");

    public static string LanStatusNoAuth => Pick(
        "已开启但无认证(不安全)",
        "已開啟但無認證（不安全）",
        "On without authentication (insecure)");

    public static string LanStatusSharedPort => Pick(
        "已开启但与本机代理共用端口(不安全)",
        "已開啟但與本機代理共用連接埠（不安全）",
        "On, sharing the local proxy port (insecure)");

    public static string LanStatusTip => Pick(
        "显示的是当前窗口中的值,点击\"确定\"后生效。",
        "顯示的是目前視窗中的值，按「確定」後生效。",
        "Reflects the values in this window. Takes effect after clicking OK.");

    public static string LanWarnNoAuth => Pick(
        "局域网端口没有认证,同一网络内任何设备都可以使用本代理。请填写用户名和密码。",
        "區域網路連接埠沒有認證，同一網路內任何裝置都可以使用本代理。請填寫使用者名稱和密碼。",
        "The LAN port has no authentication. Any device on the network can use this proxy. Set a username and password.");

    public static string LanWarnSharedPort => Pick(
        "局域网与本机代理共用端口且没有认证,建议开启\"局域网使用新端口\"并设置用户名和密码。",
        "區域網路與本機代理共用連接埠且沒有認證，建議開啟「區域網路使用新連接埠」並設定使用者名稱和密碼。",
        "LAN shares the local proxy port without authentication. Enable the dedicated LAN port and set a username and password.");

    public static string LanShareApplyRecommended => Pick("应用推荐设置", "套用推薦設定", "Apply recommended settings");

    public static string LanShareApplyTip => Pick(
        "开启局域网、使用独立端口并启用认证。用户名或密码为空时自动生成随机值。",
        "開啟區域網路、使用獨立連接埠並啟用認證。使用者名稱或密碼為空時自動產生隨機值。",
        "Enables LAN access on a dedicated port with authentication. An empty username or password is generated randomly.");

    public static string LanShareApplied => Pick(
        "已应用到当前窗口,点击\"确定\"保存。",
        "已套用到目前視窗，按「確定」儲存。",
        "Applied to this window. Click OK to save.");

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
