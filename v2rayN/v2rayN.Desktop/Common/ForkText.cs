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
        "未找到 bin/EnableLoopback.exe,请重新下载完整发布包",
        "未找到 bin/EnableLoopback.exe，請重新下載完整發布包",
        "bin/EnableLoopback.exe not found; re-download the full release package.");

    public static string UwpLoopbackFailed => Pick("无法启动回环工具", "無法啟動回環工具", "Could not start the loopback tool.");

    public static string UwpLoopbackClosed => Pick("回环工具已关闭,无法确认是否有变更", "回環工具已關閉，無法確認是否有變更", "Loopback tool closed; could not confirm any change.");

    public static string UwpLoopbackUnchanged => Pick("回环豁免无变化,当前共 {0} 个应用", "回環豁免無變化，目前共 {0} 個應用", "Loopback exemptions unchanged: {0} apps.");

    public static string UwpLoopbackChanged => Pick("回环豁免已更新:共 {0} 个应用({1:+0;-0})", "回環豁免已更新：共 {0} 個應用（{1:+0;-0}）", "Loopback exemptions updated: {0} apps ({1:+0;-0}).");

    public static string ThemeDark => Pick("深色", "深色", "Dark");
    public static string ThemeLight => Pick("浅色", "淺色", "Light");

    public static string ShowTrafficColumns => Pick("显示流量统计列", "顯示流量統計欄", "Show traffic statistics columns");

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

    // Option hints for OptionSettingWindow (hover markers). One sentence each, checked against the code.
    public static string TipSocksPort => Pick(
        "主混合监听端口（SOCKS 与 HTTP 共用）。开启第二个端口时使用 +1，局域网新端口使用 +2。",
        "主混合監聽連接埠（SOCKS 與 HTTP 共用）。開啟第二個連接埠時使用 +1，區域網路新連接埠使用 +2。",
        "Main mixed listening port (SOCKS and HTTP share it). The second port uses +1 and the new LAN port uses +2.");

    public static string TipSecondLocalPort => Pick(
        "额外开启一个本机混合代理端口，端口为主端口 +1，只监听 127.0.0.1。",
        "額外開啟一個本機混合代理連接埠，連接埠為主連接埠 +1，只監聽 127.0.0.1。",
        "Opens one more local mixed proxy port on main port +1, listening on 127.0.0.1 only.");

    public static string TipAllowLan => Pick(
        "允许局域网其他设备连接本机代理。未开启“为局域网开启新的端口”时，主端口对所有网段开放且没有认证。",
        "允許區域網路其他裝置連線本機代理。未開啟「為區域網路開啟新的連接埠」時，主連接埠對所有網段開放且沒有認證。",
        "Lets other LAN devices use the local proxy. Without the new LAN port option, the main port is open to every network with no authentication.");

    public static string TipNewPort4Lan => Pick(
        "与“允许来自局域网的连接”一起使用：额外开放主端口 +2 给局域网（监听所有地址），用户名和密码只作用于该端口。主端口保持仅本机可用。",
        "與「允許來自區域網路的連線」一起使用：額外開放主連接埠 +2 給區域網路（監聽所有位址），使用者名稱與密碼只作用於該連接埠。主連接埠維持僅本機可用。",
        "Used with Allow LAN connections: opens main port +2 to the LAN on all addresses. The username and password apply only to that port; the main port stays local-only.");

    public static string TipLanUser => Pick(
        "局域网新端口的认证用户名。仅在上一项开启且用户名和密码都不为空时生效。",
        "區域網路新連接埠的認證使用者名稱。僅在上一項開啟且使用者名稱與密碼都不為空時生效。",
        "Username for the LAN port. Applies only when the LAN port option is on and both username and password are filled in.");

    public static string TipLanPass => Pick(
        "局域网新端口的认证密码，以明文保存在配置文件中。",
        "區域網路新連接埠的認證密碼，以明文保存在設定檔中。",
        "Password for the LAN port. It is stored in plain text in the config file.");

    public static string TipSniffing => Pick(
        "根据连接内容识别目标域名（如 HTTP、TLS、QUIC 协议），用于按域名匹配路由规则。",
        "根據連線內容辨識目標網域名稱（如 HTTP、TLS、QUIC 協定），用於依網域比對路由規則。",
        "Reads the target domain from the traffic (for example HTTP, TLS or QUIC) so routing rules can match by domain.");

    public static string TipDestOverride => Pick(
        "选择参与域名识别的协议。sing-box 只使用其中的 http、tls、quic，并总是追加 dns；Xray 开启 FakeIP 时会自动加入 fakedns。",
        "選擇參與網域辨識的協定。sing-box 只使用其中的 http、tls、quic，並一律加上 dns；Xray 開啟 FakeIP 時會自動加入 fakedns。",
        "Protocols used for domain sniffing. sing-box uses only http, tls and quic and always adds dns. Xray adds fakedns when FakeIP is on.");

    public static string TipRouteOnly => Pick(
        "仅 Xray：探测到的域名只用于路由匹配，不改写连接的目标地址。TUN 模式下总是开启。",
        "僅 Xray：偵測到的網域只用於路由比對，不改寫連線的目標位址。TUN 模式下一律開啟。",
        "Xray only: sniffed domains are used for routing only and the connection target is not rewritten. Always on in TUN mode.");

    public static string TipUdpEnabled => Pick(
        "仅 Xray：控制本地入站是否接受 UDP 流量。sing-box 不使用此项。",
        "僅 Xray：控制本機入站是否接受 UDP 流量。sing-box 不使用此項。",
        "Xray only: controls whether the local inbound accepts UDP traffic. sing-box ignores this.");

    public static string TipDefFingerprint => Pick(
        "节点未设置指纹时，Xray 的 TLS 与 REALITY 出站使用此默认 TLS 指纹。sing-box 的 TLS 节点不使用此项。",
        "節點未設定指紋時，Xray 的 TLS 與 REALITY 出站使用此預設 TLS 指紋。sing-box 的 TLS 節點不使用此項。",
        "Default TLS fingerprint for Xray TLS and REALITY outbounds when a node has none. sing-box TLS nodes ignore it.");

    public static string TipDefUserAgent => Pick(
        "作为 User-Agent 伪装使用的默认值，对 raw/http、WebSocket、HTTPUpgrade 和 Xray 的 gRPC 生效；Xray 的 XHTTP 不使用。",
        "作為 User-Agent 偽裝使用的預設值，對 raw/http、WebSocket、HTTPUpgrade 與 Xray 的 gRPC 生效；Xray 的 XHTTP 不使用。",
        "Default User-Agent for disguise. Used by raw/http, WebSocket, HTTPUpgrade and Xray gRPC; Xray XHTTP does not use it.");

    public static string TipMux4Sbox => Pick(
        "sing-box 节点开启多路复用时使用的协议（h2mux、smux、yamux）。只有节点自身开启 Mux 时才生效。",
        "sing-box 節點開啟多路復用時使用的協定（h2mux、smux、yamux）。只有節點本身開啟 Mux 時才生效。",
        "Multiplexing protocol for sing-box nodes (h2mux, smux or yamux). Used only when the node has Mux enabled.");

    public static string TipMux4RayConcurrency => Pick(
        "Xray 节点开启 Mux 时的 TCP 并发数，默认 8。仅 VMess 与不带 flow 的 VLESS 生效；小于等于 0 时不写入。",
        "Xray 節點開啟 Mux 時的 TCP 並行數，預設 8。僅 VMess 與不帶 flow 的 VLESS 生效；小於等於 0 時不寫入。",
        "TCP concurrency for Xray Mux on a node, default 8. Applies to VMess and VLESS without flow; values of 0 or less are not written.");

    public static string TipMux4RayXudpConcurrency => Pick(
        "Xray Mux 中 UDP（XUDP）的并发数，默认 16。VLESS 带 flow 时只有 UDP 走 Mux。",
        "Xray Mux 中 UDP（XUDP）的並行數，預設 16。VLESS 帶 flow 時只有 UDP 走 Mux。",
        "Concurrency for UDP (XUDP) in Xray Mux, default 16. For VLESS with flow, only UDP goes through Mux.");

    public static string TipHysteriaBandwidth => Pick(
        "Hysteria2 节点未单独设置带宽时使用的上行（左）和下行（右）上限，单位 Mbps，默认 100。",
        "Hysteria2 節點未單獨設定頻寬時使用的上行（左）與下行（右）上限，單位 Mbps，預設 100。",
        "Upload (left) and download (right) limits in Mbps, used by Hysteria2 nodes that do not set their own. Default 100.");

    public static string TipEnableFragment => Pick(
        "对 TLS 出站启用 TCP 分片（Xray 使用 finalmask，sing-box 使用 TLS 分片），可能影响连接兼容性。默认关闭，展开可设置分片参数。",
        "對 TLS 出站啟用 TCP 分片（Xray 使用 finalmask，sing-box 使用 TLS 分片），可能影響連線相容性。預設關閉，展開可設定分片參數。",
        "Enables TCP fragmentation on TLS outbounds (Xray finalmask, sing-box TLS fragmentation). It can affect compatibility. Default off; expand to set parameters.");

    public static string TipFragmentDelays => Pick(
        "分片之间的延迟范围（毫秒），如 10-20。留空使用默认值 10-20。",
        "分片之間的延遲範圍（毫秒），如 10-20。留空使用預設值 10-20。",
        "Delay range between fragments in milliseconds, for example 10-20. Empty uses the default 10-20.");

    public static string TipLogToFile => Pick(
        "把核心（Xray 或 sing-box）的访问日志和错误日志写入日志目录的文件。不影响 v2rayN 自身的日志。",
        "把核心（Xray 或 sing-box）的存取日誌與錯誤日誌寫入日誌目錄的檔案。不影響 v2rayN 自身的日誌。",
        "Writes core (Xray or sing-box) access and error logs to files in the log folder. v2rayN's own log is not affected.");

    public static string TipLogLevel => Pick(
        "核心日志级别：debug、info、warning、error。选择 none 时关闭日志；sing-box 会整体禁用日志输出。",
        "核心日誌等級：debug、info、warning、error。選擇 none 時關閉日誌；sing-box 會整體停用日誌輸出。",
        "Core log level: debug, info, warning or error. none turns logging off; sing-box disables log output entirely.");

    public static string TipCacheFile4Sbox => Pick(
        "sing-box 将缓存写入 cache.db，开启 FakeIP 时同时保存 FakeIP 映射。",
        "sing-box 將快取寫入 cache.db，開啟 FakeIP 時同時保存 FakeIP 對應。",
        "sing-box writes its cache to cache.db, which also stores FakeIP mappings when FakeIP is on.");

    public static string TipAutoRun => Pick(
        "登录系统后自动启动 v2rayN。Windows 上通过计划任务实现，设置可能需要管理员权限。",
        "登入系統後自動啟動 v2rayN。Windows 上透過工作排程器實現，設定可能需要系統管理員權限。",
        "Starts v2rayN when you sign in. On Windows this uses a scheduled task, which may need administrator rights.");

    public static string TipAutoHide => Pick(
        "v2rayN 启动后隐藏主窗口，只保留托盘图标。",
        "v2rayN 啟動後隱藏主視窗，只保留系統匣圖示。",
        "Hides the main window after v2rayN starts, leaving only the tray icon.");

    public static string TipDoubleClick => Pick(
        "开启时双击节点列表的行会把该节点设为活动节点；关闭时双击打开编辑对话框。",
        "開啟時雙擊節點列表的列會把該節點設為使用中節點；關閉時雙擊開啟編輯對話框。",
        "On: double-clicking a node row makes it the active node. Off: double-click opens the edit dialog.");

    public static string TipOrientation => Pick(
        "主界面的分栏方式：水平、垂直或标签页。需重启后生效。",
        "主介面的分欄方式：水平、垂直或標籤頁。需重新啟動後生效。",
        "How the main window is split: horizontal, vertical or tabs. Takes effect after restart.");

    public static string TipRealtimeSpeed => Pick(
        "在状态栏显示实时上传和下载速度。开启后会启动流量统计采集，需重启后生效。",
        "在狀態列顯示即時上傳與下載速度。開啟後會啟動流量統計收集，需重新啟動後生效。",
        "Shows live upload and download speed in the status bar. Starts traffic statistics collection. Takes effect after restart.");

    public static string TipHwa => Pick(
        "仅 Windows：关闭时使用软件渲染（默认）；开启后使用图形加速渲染。需重启后生效。",
        "僅 Windows：關閉時使用軟體算繪（預設）；開啟後使用圖形加速算繪。需重新啟動後生效。",
        "Windows only: off (default) uses software rendering; on uses GPU rendering. Takes effect after restart.");

    public static string TipFontFamily => Pick(
        "界面控件使用的字体名称，可从列表选择或直接输入。留空使用系统默认字体。需重启后生效。",
        "介面控制項使用的字型名稱，可從清單選擇或直接輸入。留空使用系統預設字型。需重新啟動後生效。",
        "Font name used by the interface. Pick from the list or type one. Leave empty for the system font. Takes effect after restart.");

    public static string TipHide2Tray => Pick(
        "仅 Linux：开启后关闭窗口会隐藏到托盘；关闭时关闭窗口只会最小化。仅在系统有托盘时开启。",
        "僅 Linux：開啟後關閉視窗會隱藏到系統匣；關閉時關閉視窗只會最小化。僅在系統有系統匣時開啟。",
        "Linux only: when on, closing the window hides it to the tray; when off, closing only minimizes it. Enable only if your desktop has a tray.");

    public static string TipMacDock => Pick(
        "仅 macOS：在 Dock 中显示图标。启动时读取，需重启后生效。",
        "僅 macOS：在 Dock 中顯示圖示。啟動時讀取，需重新啟動後生效。",
        "macOS only: shows the icon in the Dock. It is read at startup, so restart to apply.");

    public static string TipTrayLimit => Pick(
        "节点数量超过此值时，托盘右键菜单不显示服务器列表（整体隐藏，不截断）。",
        "節點數量超過此值時，系統匣右鍵選單不顯示伺服器列表（整體隱藏，不截斷）。",
        "When the node count is above this value, the tray menu hides the server list entirely instead of trimming it.");

    public static string TipDragDrop => Pick(
        "开启后可在节点列表中拖动调整顺序。需重启后生效。",
        "開啟後可在節點列表中拖曳調整順序。需重新啟動後生效。",
        "Lets you drag rows in the node list to reorder them. Takes effect after restart.");

    public static string TipAutoAdjust => Pick(
        "订阅更新成功后，自动调整节点列表的列宽。",
        "訂閱更新成功後，自動調整節點列表的欄寬。",
        "After a successful subscription update, adjusts the node list column widths automatically.");

    public static string TipStatistics => Pick(
        "采集节点流量统计，在节点列表中显示统计列，并影响排序。需重启后生效。",
        "收集節點流量統計，在節點列表中顯示統計欄，並影響排序。需重新啟動後生效。",
        "Collects per-node traffic statistics, shows a statistics column in the node list, and affects sorting. Takes effect after restart.");

    public static string TipSpeedTestUrl => Pick(
        "下载测速使用的文件地址，请求经节点代理发出。默认是 50 MB 测试文件。",
        "下載測速使用的檔案位址，請求經節點代理發出。預設是 50 MB 測試檔案。",
        "File URL for download speed tests, requested through each node's proxy. Default is a 50 MB test file.");

    public static string TipSpeedPingUrl => Pick(
        "真连接延迟测试使用的地址。负载均衡的探测也会使用此地址。",
        "真連線延遲測試使用的位址。負載平衡的探測也會使用此位址。",
        "URL for the real-connection latency test. Load-balancer probes also use it.");

    public static string TipUdpTest => Pick(
        "UDP 延迟测试的目标，格式如 ntp:主机名，默认 ntp:pool.ntp.org。",
        "UDP 延遲測試的目標，格式如 ntp:主機名稱，預設 ntp:pool.ntp.org。",
        "Target for the UDP latency test, in the form ntp:host. Default ntp:pool.ntp.org.");

    public static string TipSpeedTestTimeout => Pick(
        "单个节点下载测速的整体超时时间（秒），最小为 10。",
        "單一節點下載測速的整體逾時時間（秒），最小為 10。",
        "Overall timeout in seconds for one node's download test. Minimum 10.");

    public static string TipMixedConcurrency => Pick(
        "混合测速时的并发数量，按批执行。最小为 10。",
        "混合測速時的並行數量，分批執行。最小為 10。",
        "How many nodes are tested at once in a mixed test, run in batches. Minimum 10.");

    public static string TipIpApi => Pick(
        "用于查询节点出口 IP 信息的地址。留空则不查询，并隐藏 IP 信息列。",
        "用於查詢節點出口 IP 資訊的位址。留空則不查詢，並隱藏 IP 資訊欄。",
        "URL used to look up each node's exit IP information. Leave empty to skip lookups and hide the IP info column.");

    public static string TipSubConvert => Pick(
        "订阅格式转换服务地址，{0} 会被替换为原订阅地址（URL 编码）。留空使用内置地址。",
        "訂閱格式轉換服務位址，{0} 會被替換為原訂閱位址（URL 編碼）。留空使用內建位址。",
        "Subscription conversion service URL. {0} is replaced with the URL-encoded subscription address. Empty uses the built-in address.");

    public static string TipAutoUpdate => Pick(
        "每隔 N 小时自动更新 Geo 文件，0 表示关闭。",
        "每隔 N 小時自動更新 Geo 檔案，0 表示關閉。",
        "Updates the Geo files every N hours. 0 turns automatic updates off.");

    public static string TipKeepOlder => Pick(
        "订阅去重时，开启则保留列表中靠前的节点，关闭则保留靠后的节点。",
        "訂閱去重時，開啟則保留清單中靠前的節點，關閉則保留靠後的節點。",
        "When subscription duplicates are removed: on keeps the node earlier in the list, off keeps the later one.");

    public static string TipGeoSource => Pick(
        "Geo 文件（geosite、geoip）下载地址模板，{0} 会被替换为文件名。留空使用内置地址。",
        "Geo 檔案（geosite、geoip）下載位址模板，{0} 會被替換為檔名。留空使用內建位址。",
        "Download URL template for Geo files (geosite, geoip). {0} is replaced with the file name. Empty uses the built-in URL.");

    public static string TipSrsSource => Pick(
        "sing-box 规则集下载地址模板。留空使用内置地址。",
        "sing-box 規則集下載位址模板。留空使用內建位址。",
        "Download URL template for sing-box rule sets. Empty uses the built-in URL.");

    public static string TipRoutingSource => Pick(
        "路由规则模板地址。留空则使用内置路由规则；填写后会下载该模板。",
        "路由規則模板位址。留空則使用內建路由規則；填寫後會下載該模板。",
        "URL of a routing rule template. Empty uses the built-in routing rules; if set, the template is downloaded.");

    public static string TipScriptPath => Pick(
        "Linux 和 macOS 设置系统代理时调用的脚本路径。留空使用内置方式。",
        "Linux 與 macOS 設定系統代理時呼叫的腳本路徑。留空使用內建方式。",
        "Script run to set the system proxy on Linux and macOS. Empty uses the built-in method.");

    public static string TipNotProxyLocal => Pick(
        "仅 Windows：在代理例外列表中加入 <local>，本地地址不走代理。默认开启。",
        "僅 Windows：在代理例外清單中加入 <local>，本機位址不走代理。預設開啟。",
        "Windows only: adds <local> to the proxy exceptions so local addresses bypass the proxy. Default on.");

    public static string TipAdvancedProtocol => Pick(
        "仅 Windows：系统代理字符串模板。{ip} 为 127.0.0.1，{http_port} 与 {socks_port} 都会替换为主端口。留空为普通代理。",
        "僅 Windows：系統代理字串模板。{ip} 為 127.0.0.1，{http_port} 與 {socks_port} 都會替換為主連接埠。留空為一般代理。",
        "Windows only: template for the system proxy string. {ip} is 127.0.0.1, and {http_port} and {socks_port} both become the main port. Empty means a normal proxy.");

    public static string TipPacPath => Pick(
        "PAC 文件路径。文件存在时使用它，否则使用内置的 pac.txt。",
        "PAC 檔案路徑。檔案存在時使用它，否則使用內建的 pac.txt。",
        "Path to a PAC file. If it exists it is served; otherwise the built-in pac.txt is used.");

    public static string TipTunAutoRoute => Pick(
        "仅 sing-box TUN 生效：由 sing-box 自动配置系统路由。Xray 原生 TUN 下无效。",
        "僅 sing-box TUN 生效：由 sing-box 自動設定系統路由。Xray 原生 TUN 下無效。",
        "sing-box TUN only: sing-box sets up the system routes automatically. No effect with native Xray TUN.");

    public static string TipTunStrictRoute => Pick(
        "阻止 TUN 流量泄漏。Xray 下仅 Windows 生效。",
        "阻止 TUN 流量外洩。Xray 下僅 Windows 生效。",
        "Stops TUN traffic from leaking outside the tunnel. With Xray it only works on Windows.");

    public static string TipTunStack => Pick(
        "仅 sing-box TUN 使用：网络栈实现。留空时不写入，由 sing-box 决定。",
        "僅 sing-box TUN 使用：網路堆疊實作。留空時不寫入，由 sing-box 決定。",
        "sing-box TUN only: the network stack implementation. Empty means not written, so sing-box decides.");

    public static string TipTunMtu => Pick(
        "TUN 接口的 MTU，可从列表选择或输入。值小于等于 0 时运行时回退为 1280。",
        "TUN 介面的 MTU，可從清單選擇或輸入。值小於等於 0 時執行時回退為 1280。",
        "MTU of the TUN interface. Pick from the list or type a value. A value of 0 or less falls back to 1280 at runtime.");

    public static string TipTunIcmp => Pick(
        "仅 sing-box TUN 生效：ICMP（ping）的处理方式。direct 直连，rule 按路由规则处理，其它值拒绝。",
        "僅 sing-box TUN 生效：ICMP（ping）的處理方式。direct 直連，rule 依路由規則處理，其他值拒絕。",
        "sing-box TUN only: how ICMP (ping) is handled. direct sends it direct, rule follows the routing rules, other values reject it.");

    public static string TipTunEnableIpv6 => Pick(
        "为 TUN 接口增加 IPv6 地址。它只影响接口地址，路由是否包含 IPv6 默认路由取决于本机是否有全局 IPv6。",
        "為 TUN 介面增加 IPv6 位址。它只影響介面位址，路由是否包含 IPv6 預設路由取決於本機是否有全域 IPv6。",
        "Adds an IPv6 address to the TUN interface. It does not change routes; IPv6 default routes depend on whether this machine has global IPv6.");

    public static string TipTunIpv4 => Pick(
        "TUN 接口的 IPv4 地址（CIDR 格式）。留空使用 172.18.0.1/30。",
        "TUN 介面的 IPv4 位址（CIDR 格式）。留空使用 172.18.0.1/30。",
        "IPv4 address of the TUN interface in CIDR form. Empty uses 172.18.0.1/30.");

    public static string TipTunIpv6 => Pick(
        "TUN 接口的 IPv6 地址（CIDR 格式），仅在开启 IPv6 时写入。留空使用 fc00::172:18:0:1/126。",
        "TUN 介面的 IPv6 位址（CIDR 格式），僅在開啟 IPv6 時寫入。留空使用 fc00::172:18:0:1/126。",
        "IPv6 address of the TUN interface in CIDR form, written only when IPv6 is on. Empty uses fc00::172:18:0:1/126.");

    public static string TipLegacyProtect => Pick(
        "TUN 开启且运行核心为 Xray 时，额外用 sing-box 作为前置 SOCKS 接管 TUN，Xray 经本地 SOCKS 接力。默认开启；Custom 节点仅在配置了前置端口时受影响。",
        "TUN 開啟且執行核心為 Xray 時，額外用 sing-box 作為前置 SOCKS 接管 TUN，Xray 經本機 SOCKS 接力。預設開啟；Custom 節點僅在設定了前置連接埠時受影響。",
        "With TUN on and Xray as the core, sing-box runs as a front SOCKS that takes over TUN, and Xray relays through the local SOCKS. On by default; Custom nodes are affected only when a pre-SOCKS port is set.");

    public static string TipRouteExclude => Pick(
        "TUN 路由排除的 CIDR 列表，用逗号分隔。无效地址会被忽略并记录警告。",
        "TUN 路由排除的 CIDR 清單，以逗號分隔。無效位址會被忽略並記錄警告。",
        "CIDR list excluded from TUN routing, separated by commas. Invalid entries are ignored and logged as warnings.");

    public static string TipCoreVmess => Pick(
        "VMess 类型节点使用的内核（Xray 或 sing-box）。",
        "VMess 類型節點使用的核心（Xray 或 sing-box）。",
        "Core (Xray or sing-box) that runs VMess nodes.");

    public static string TipCoreCustom => Pick(
        "Custom 类型节点设置了前置 SOCKS 端口（PreSocksPort）时，前置实例使用的内核。",
        "Custom 類型節點設定了前置 SOCKS 連接埠（PreSocksPort）時，前置執行個體使用的核心。",
        "Core for the front SOCKS instance of Custom nodes that set a PreSocksPort.");

    public static string TipCoreShadowsocks => Pick(
        "Shadowsocks 类型节点使用的内核（Xray 或 sing-box）。",
        "Shadowsocks 類型節點使用的核心（Xray 或 sing-box）。",
        "Core (Xray or sing-box) that runs Shadowsocks nodes.");

    public static string TipCoreSocks => Pick(
        "SOCKS 类型节点使用的内核（Xray 或 sing-box）。",
        "SOCKS 類型節點使用的核心（Xray 或 sing-box）。",
        "Core (Xray or sing-box) that runs SOCKS nodes.");

    public static string TipCoreVless => Pick(
        "VLESS 类型节点使用的内核（Xray 或 sing-box）。",
        "VLESS 類型節點使用的核心（Xray 或 sing-box）。",
        "Core (Xray or sing-box) that runs VLESS nodes.");

    public static string TipCoreTrojan => Pick(
        "Trojan 类型节点使用的内核（Xray 或 sing-box）。",
        "Trojan 類型節點使用的核心（Xray 或 sing-box）。",
        "Core (Xray or sing-box) that runs Trojan nodes.");

    public static string TipCoreHysteria2 => Pick(
        "Hysteria2 类型节点使用的内核（Xray 或 sing-box）。",
        "Hysteria2 類型節點使用的核心（Xray 或 sing-box）。",
        "Core (Xray or sing-box) that runs Hysteria2 nodes.");

    public static string TipCoreWireguard => Pick(
        "WireGuard 类型节点使用的内核（Xray 或 sing-box）。",
        "WireGuard 類型節點使用的核心（Xray 或 sing-box）。",
        "Core (Xray or sing-box) that runs WireGuard nodes.");

    public static string TipCoreMasque => Pick(
        "MASQUE 类型节点使用的内核（Xray 或 sing-box）。",
        "MASQUE 類型節點使用的核心（Xray 或 sing-box）。",
        "Core (Xray or sing-box) that runs MASQUE nodes.");

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
