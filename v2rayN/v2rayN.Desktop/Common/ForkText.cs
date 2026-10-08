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
