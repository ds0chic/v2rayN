# v2rayN（ds0chic fork）代码审计报告

- 审计对象：`master` @ `0043892`。这是只保留 Avalonia 前端的 fork：WPF 已删除，唯一的 UI 项目是 `v2rayN.Desktop`，fork 补丁登记在 `FORK_PATCHES.md`。
- 范围：ServiceLib 核心路径（ViewModels、Manager、SpeedtestService、ConfigHandler、Statistics、PAC、Clash API）、`v2rayN.Desktop` 视图，以及 fork 新增或修改的代码（TUN、LAN 共享、路由升级、sing-box 嗅探、AI 自动化、托盘）。
- 关注点：① 前端性能热点 ② 正确性与稳定性 ③ 安全 ④ 对 Flutter 重构的影响
- 方法：人工读代码。**本环境没有 .NET SDK，未编译、未运行测试**；每条结论都给出了代码位置和触发路径，可直接复核。
- 历史：初版基于上游 `5ea8ae6`（当时还有 WPF）。本版在 fork `master` 上逐条复核：H1 已被 fork 补丁修复，其余问题仍然存在（相关文件在 fork 中只删除了注释）；另外新增了针对 fork 代码的 F 系列发现。

## 总览

| 级别 | 未修复 | 说明 |
|---|---|---|
| 高 | 2 | H2、H3（H1 已由 fork 补丁修复） |
| 中 | 10 | M1–M8、F1、F2 |
| 低 | 9 | L1–L6、F3–F5 |
| 性能热点 | 5 | P1–P5，大订阅（数千节点）下卡顿的主要来源 |

---

## 一、高

### ~~H1 Windows 开启 TUN 时取消 UAC，程序直接退出~~（已修复）
- fork 补丁「UAC decline keeps app running」：`AppManager.RebootAsAdmin()` 现在返回 `bool`，`StatusBarViewModel.DoEnableTun` 在提权被拒后回滚开关并提示，菜单入口也检查了返回值。复核无误。

### H2 PAC 服务一次连接异常即永久停止，且监听所有网卡
- 位置：`ServiceLib/Manager/PacManager.cs:23, 61-85`
- 问题 1：`try` 包住整个 `while` 循环，任何一个客户端提前断开（`IOException`）都会跳出循环并 `listener.Stop()`，
  PAC 服务静默死亡，直到重启程序。系统代理「PAC 模式」随之失效。
- 问题 2：串行 accept → `ReadAsync` 无超时。一个只连接不发数据的客户端会把服务永久卡住。
- 问题 3：`TcpListener.Create(port)` 绑定的是 IPv6Any 双栈，即对局域网开放，配合问题 2 可被局域网任意主机卡死。
- 修复：只绑定 `IPAddress.Loopback`；每个连接独立 `try/catch` 并 `Task.Run` 处理，读取加 5 秒超时。

### H3 测速时多线程并发写非线程安全的 `Queue<string>`
- 位置：`ServiceLib/Manager/ProfileExManager.cs:7, 32-38`
- 路径：`SpeedtestService` 的 `Parallel.ForEachAsync`（并发度最高 = 页大小 1000）里调用
  `SetTestDelay / SetTestMessage / SetTestIpInfo` → `IndexIdEnqueue` → 对普通 `Queue<string>` 并发 `Contains + Enqueue`。
- 结果：内部数组损坏、丢失条目或抛异常。异常在并发循环里被 `catch` 吞掉，导致该节点的 `UpdateFunc` 没执行，
  界面停在「测试中」，测速结果也可能不落库。
- 修复：改用 `ConcurrentDictionary<string, byte>` 作为脏标记集合（同时解决性能热点 P1）。

---

## 二、中

### F1（fork）路由升级的签名漏掉了 Process、InboundTag、RuleType，用户对这些字段的修改会被静默丢弃
- 位置：`ServiceLib/Handler/ForkRoutingUpgrade.cs:25-32`（`RuleContent`）
- `RuleContent` 只包含 OutboundTag、Port、Network、Protocol、Domain、Ip。但 `RoutingRuleDetailsViewModel` 允许编辑
  `Process`、`InboundTag`、`RuleType`（见该文件第 92-102 行）。
- 如果用户只改了内置规则的这几个字段（例如加了进程名，或把规则限定为 DNS），这条规则的签名仍然等于原模板：
  合并时会被当作「未修改的模板规则」丢掉；如果整组规则都只有这类改动，整组签名等于旧签名，整组会被直接替换。
  迁移只运行一次，丢失的修改无法找回。
- 修复：把 `Type`、`InboundTag`、`Process`、`RuleType` 也加进 `RuleContent`，然后重新计算 `OldSignature` 和 `OldRuleSignatures`。
  原模板这些字段为空，所以重算后的旧签名只是格式变化，判断逻辑不变。

### F2（fork）sing-box 嗅探列表收窄后，黑名单的「绕过 bittorrent」规则在 sing-box 下失效
- 位置：`ServiceLib/Services/CoreConfig/Singbox/SingboxRoutingService.cs:121, 610-623`（`BuildSniffers`）
- 默认 `DestOverride = ["http", "tls"]`（`ConfigItems.cs:32`），所以 sniff 规则的 `sniffer` 变成 `[http, tls, dns]`。
  以前不传 `sniffer` 时 sing-box 会嗅探所有协议；现在 bittorrent 不再被识别。
- `Sample/custom_routing_black` 第一条规则是 `protocol: ["bittorrent"] → direct`，在 sing-box 下永远匹配不到。
  BT 流量会按目标地址走代理，很多机场会因此封号。用户自己写的 `protocol` 规则（stun、quic 等）同样受影响。
  Xray 的语义是 `destOverride` 只决定是否覆盖目标地址，不影响协议识别，所以两个内核的行为现在不一致。
- 现有测试 `CoreConfigSingboxServiceTests.cs:827-831` 反而把 `[http, tls, dns]` 固定成了预期结果。
- 修复：在 `BuildSniffers` 中把当前路由规则里出现的所有 `Protocol` 值都加进列表（至少始终包含 `bittorrent`），并补一条测试。

### M1 TCPing / 真连接测速会漏掉非 Xray、非 sing-box 内核的节点，并永远显示「测试中」
- 位置：`ServiceLib/Services/SpeedtestService.cs:587-603`（`GetTestBatchItem`）
- 只收集 `CoreType == Xray` 和 `== sing_box` 的节点；为节点或协议单独指定了 v2fly、mihomo、hysteria2、naiveproxy 等内核的节点被丢弃。
  这些节点已在 `GetClearItem` 中被标为「测试中」，之后没有任何更新。TCPing 根本不需要内核，这里属于误过滤。

### M2 TCPing 的 DNS 失败会让节点卡在「测试中」，且 DNS 无超时
- 位置：`SpeedtestService.cs:558-563`、`:228-235`
- `Dns.GetHostEntryAsync` 抛异常 → 被 `catch (Exception)` 只记日志，不调用 `UpdateFunc(-1)`。
- DNS 解析不在 5 秒超时内；`AddressList.First()` 可能拿到 IPv6 地址，在无 IPv6 网络下必然失败。

### M3 真连接测速的「失败重测」退化成重测整批
- 位置：`SpeedtestService.cs:269-276`
- 当 `pageSize / 2 <= MixedConcurrencyCount` 时调用的是 `RunMixedTestAsync(lstSelected, ...)`，应为 `lstFailed`。
  结果是已成功的节点再逐个起内核测一遍，耗时成倍增加。

### M4 「按测试结果排序」后失败节点没有沉底，首次排序时甚至排在最前
- 位置：`ServiceLib/Handler/ConfigHandler.cs:1111, 1121`，以及 `:1256`
- `maxSort = lstProfile.Max(t => t.Sort) + 10` 取的是 join 时拷贝的**旧** Sort 值，而不是刚分配的 `(i+1)*10`。
- 导入订阅时 `AddServerCommon` 给的是 `GetMaxSort() + 1`（还有 `maxSort > 0` 才赋值），
  所以新装或首次导入后 Sort 往往是 0 或很小的连续整数。此时失败节点被设为很小的 maxSort，落在列表顶部或中间。
- 修复：用 `lstProfile.Count * 10 + 10`。

### M5 日志过滤填了非法正则后，每一行日志都往日志文件写一条异常堆栈
- 位置：`ServiceLib/ViewModels/MsgViewModel.cs:76-90`，`ServiceLib/Common/Utils.cs:726-730`
- `Utils.IsRegexMatch` 自己捕获 `ArgumentException` 并返回 `true`，所以 `MsgViewModel` 里设置
  `_lastMsgFilterNotAvailable` 的 `catch` 永远不会进入。每行日志都重新解析正则并 `Logging.SaveLog(..., ex)`。
- 另外：匹配在发布线程上、在 `EventChannel` 的锁内执行，超时 2 秒；病态正则会拖慢内核日志的读取线程。

### M6 Clash API 无鉴权，且会删除用户自定义的 secret
- 位置：`Services/CoreConfig/Singbox/SingboxStatisticService.cs:12`，`Services/CoreConfig/CoreConfigClashService.cs:80-81`
- sing-box / mihomo 的 `external_controller` 绑定 127.0.0.1、端口可预测（基于 10808 偏移）、没有 secret。
  本机任何进程都可以切换节点、读取连接列表；浏览器通过 DNS rebinding 也可能访问到。
- 修复：每次启动生成随机 secret，写进配置，并在 `ClashApiManager` 的请求头里带上。

### M7 `SaveConfig` 无并发保护
- 位置：`ConfigHandler.cs:205-230`
- 18 处调用分布在 UI 线程和 `Task.Run`（Reload、订阅更新、定时任务）中；多个调用同时写同一个 `guiNConfig.json_temp`，
  会出现 `IOException` 或 `File.Move` 失败。另外，序列化期间其他线程修改 `config` 里的 List 会抛
  「Collection was modified」。失败只记日志、返回 -1，用户无感知，设置可能丢失。
  fork 的 TUN 补丁在 `DoEnableTun` 里又增加了两次 `SaveConfig`。
- 修复：静态 `SemaphoreSlim` 串行化，并使用唯一临时文件名。

### M8 `StatisticsManager._lstServerStat` 跨线程读写
- 位置：`ServiceLib/Manager/StatisticsManager.cs:87, 152`；读取方 `ProfilesViewModel.cs` 中 `GetProfileItemsEx` 的 join
- 统计后台线程在切换到新节点时 `Add`，UI 线程同时在 join 中枚举，可能抛 `InvalidOperationException`，导致列表刷新失败。

---

## 三、低

| # | 问题 | 位置 |
|---|---|---|
| L1 | 有过滤条件时上下移动节点，只对可见子集重编号为 10、20…，会与隐藏节点的 Sort 冲突，打乱整体顺序 | `ConfigHandler.cs:485-488` |
| L2 | 节点列表 SQL 用字符串拼接，只删掉了单引号；`%`、`_` 没有转义，`subid` 未转义。本地应用风险低，但应改为参数化查询 | `AppManager.cs:219-227` |
| L3 | 订阅解析失败时把整个订阅内容写进日志文件，可能包含节点密码或 token | `SubscriptionHandler.cs:219-220` |
| L4 | Xray 流量按 1024、sing-box 按 1000 换算，两者混存在同一张统计表；sing-box 的 `/traffic` 是总流量，会把直连流量计入节点统计 | `StatisticsXrayService.cs:91`，`StatisticsSingboxService.cs:81` |
| L5 | 日志面板超过 500 行时整段清空（显示 “Message cleared”）；过滤条件只对新日志生效，已有内容不会重新过滤 | `v2rayN.Desktop/Views/MsgView.axaml.cs:34` |
| L6 | 程序和内核的自动更新只依赖 TLS，没有校验 SHA256 或签名（Release 本身有 GPG 签名，但程序内升级不校验） | `Services/UpdateService.cs` |
| F3（fork） | LAN 共享的防火墙规则没有限制 `remoteip=localsubnet` 和 `profile`，在公共网络下也会放行；状态为「无认证」或「共用端口」时等于把代理暴露给同一热点的所有人。另外 `netsh` 没用绝对路径（`pnputil` 用了），输出按 UTF-8 解码，中文系统下结果会乱码 | `v2rayN.Desktop/Common/LanShareHelper.cs:69-73, 93-100` |
| F4（fork） | AI 自动化管道：ACL 只认用户 SID，不区分完整性级别。如果实例以管理员运行（TUN），同一用户的普通权限进程也能驱动这个提权窗口。建议提权时拒绝启动，并加 `PipeOptions.FirstPipeInstance`。该功能需显式开启、仅供测试用，所以只算低危 | `v2rayN.Desktop/Common/AiAutomation.cs:26-29, 68` |
| F5（fork） | 开启 TUN 时每次 Reload 要调用 `RemoveTunDevice` 两次（`CoreStop` 里一次，`LoadCore` 里又一次），每次至少启动两次 `pnputil` 做枚举，增加数百毫秒切换延迟；第二次调用是多余的 | `ServiceLib/Manager/CoreManager.cs:93, 185` |

---

## 四、前端性能热点（Flutter 重构要解决的核心）

fork 的重新设计只改了样式和布局，没有改数据流，下面的问题在 `v2rayN.Desktop` 中仍然存在。主题里没有给 DataGrid 行加过渡动画，这一点没问题。

### P1 ProfileExManager 的 O(n²) 查找（移动、排序、导入都会触发）
- `GetProfileExItem` 对 `ConcurrentBag` 做 `FirstOrDefault`。**每次枚举 ConcurrentBag 都会冻结并复制整个集合**；
  `IndexIdEnqueue` 又对 `Queue` 做 O(n) 的 `Contains`。
- `MoveServer`、`SortServers` 会对子集里的每个节点调用 `SetSort`；`AddServerCommon` 每导入一个节点都会调用 `GetMaxSort`（全量枚举）。
- 以 5000 个节点为例：移动一次约复制 2500 万个元素，导入 5000 个节点同样是平方级开销。`SaveQueueIndexIds` 也是 O(n²)。
- 修复：改为 `ConcurrentDictionary<string, ProfileExItem>`，并单独维护 maxSort。

### P2 测速结果逐条派发到 UI 线程 + 线性查找
- `ProfilesViewModel.cs:724-732`：每条结果单独调度一次 UI 线程；`SetSpeedTestResult`（第 273 行）用
  `ProfileItems.FirstOrDefault(...)` 做 O(n) 查找。光是开始测速时把所有节点标为「测试中」，就要 N 次调度 × N 次查找。
- 修复方向：建 `indexId → model` 字典，按 100ms 批量合并后再刷新 UI。

### P3 任何操作都会全量刷新列表
- 设为活动节点、移动、编辑、排序、删除……最终都走 `RefreshServers`：
  `ProfilesViewModel.RefreshServersBiz` 和 `StatusBarViewModel.RefreshServersBiz` 各查一次全表，再做 LINQ join，
  然后 `ReplaceRange` 触发 `Reset`，DataGrid 整体重建。结果是滚动位置跳动、选择丢失、大列表明显卡顿。
- fork 新增的 `TrayMenuManager` 监听 `Servers.CollectionChanged`，所以每次刷新还会整体重建托盘服务器菜单。
- 例如「设为活动节点」只需要翻转两行的 `IsActive`。

### P4 日志管道
- 每行日志都在内核输出线程上、在锁内执行正则匹配（见 M5），超过 500 行时整段清空（见 L5）。
- 修复方向：使用环形缓冲，按批量推送；过滤放在显示层。

### P5 流量统计
- Xray 每秒拉一次 `/debug/vars`，对每个 outbound 做两次 JSON 反序列化。
- `ProfilesViewModel.UpdateStatistics` 也是 O(n) 查找，节流靠 `DateTime.Now.Second % 3`（依赖墙钟，不稳定）。

---

## 五、可维护性（fork 同步）

- 「Dead-code cleanup」删除了很多上游文件里的注释和未使用成员（如 `Utils.cs` 少了 62 行，`Extension.cs`、`GroupProfileManager.cs`、`WindowsUtils.cs` 也有删除）。
  这和 `FORK_PATCHES.md` 中「优先新增文件、少改上游文件」的原则相反：每次合并 `upstream/master` 时，这些位置都可能冲突，而收益只是代码更整洁。
  建议今后不再做这类删除；已删除的部分，冲突时按登记表的规则以上游为准即可。
- 本报告里 ServiceLib 自身的 bug（H2、H3、M1–M8、P1），按 fork 规则修复时应：补丁尽量小、在 `FORK_PATCHES.md` 中登记意图和「上游修复后删除」的条件、附带测试。

## 六、对 Flutter 重构方案的影响

1. **Host 必须设置 `AppManager.Instance.ShowInTaskbar = true`**，否则日志不会 flush（`MsgViewModel.FlushQueueToView`），
   统计事件也会被丢弃（`MainWindowViewModel.UpdateStatisticsHandler`）。
2. **Host 不要直接复用 `ProfilesViewModel` 和 `MainWindowViewModel`**：它们自带 P2、P3 的问题。
   建议 Host 直接调用 `AppManager`、`ConfigHandler`、`SpeedtestService`、`CoreManager`，
   自己维护 `indexId → 行` 的映射，WebSocket 只推增量，测速结果按 100ms 批量推送。这样 ServiceLib 不用改，符合 fork「优先新增文件」的原则。
3. UI 耦合点：9 个 ViewModel 共 24 个 `Interaction<>`，19 处 `WindowDialog.ShowDialogAsync`。
   第一阶段（节点列表、状态栏、日志）只需要实现其中约 6 个。
4. **fork 的 Desktop 专属功能需要在 Flutter 里重做**：LAN 共享卡片（`LanShareCard`、`LanShareHelper`）、右下角 toast、托盘菜单（`TrayMenuManager`）、
   窗口防闪白（`WindowCloakHelper`）、UWP 回环、设置项悬停说明（`ForkText.Tip*`）。其中纯逻辑（`ServiceLib/Common/LanShare.cs`）可以直接经 Host 复用。
   界面文字除了 `ResUI.*.resx`，还要导出 `ForkText.cs` 里的中、繁、英字符串。

## 七、建议的处理顺序

1. F2、H2、H3、M3、M4、M5：每个修复都在 20 行以内，影响面小。F2 会影响用户的实际流量走向，应优先修。
2. F1、M1、M2、M6、M7、M8：修复稍多，需要配合测试。F1 应在还有大量用户停留在 `V4-` 路由时尽快修。
3. P1：改 ProfileExManager 的数据结构。
4. P2、P3、P4：在 Flutter Host 里绕开，不改 ServiceLib。
5. F3–F5：顺手加固。
