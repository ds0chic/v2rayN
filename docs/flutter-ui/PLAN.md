# v2rayN Flutter 前端重构方案（Windows，第一阶段）

## 背景
- 本仓库是 ds0chic 的 fork：**只有 Avalonia 前端**（`v2rayN.Desktop`，WPF 已删除），界面已按 fork 设计系统重做，补丁登记在 `FORK_PATCHES.md`。
- Flutter 第一阶段只做 Windows。过渡期内 Windows 上 Flutter 与 Avalonia 并存（Avalonia 作为回退），Linux/macOS 继续使用 Avalonia。
  等 Flutter 版功能对齐后，再决定是否停止发布 Avalonia 的 Windows 包。
- 现有代码的问题见 [AUDIT.md](AUDIT.md)。本方案按审计结论调整了复用方式（见「第一阶段范围」）。

## 目标
- 性能：节点列表虚拟化渲染、测速结果增量刷新、日志流不卡 UI。
- 体验：现代、轻量、美观的界面（Material 3 / Fluent 风格，深浅色），视觉延续 fork 设计规则（两档圆角 10/6、卡片、悬停说明、右下角 toast）。
- 可维护：**不修改 `ServiceLib` 任何文件**，Host 和 Flutter 客户端都是新增目录。合并 `upstream/master` 时不会因此产生冲突，符合 `FORK_PATCHES.md`「优先新增文件」的原则。

## 架构（方案 A：本地 IPC 后端）

```
┌────────────────────┐   loopback + token    ┌─────────────────────────────┐
│ Flutter (Windows)  │ ◄───────────────────► │ v2rayN.Host (.NET, headless) │
│ UI only            │  REST + WebSocket     │ 引用 ServiceLib（原样复用）   │
└────────────────────┘                       └─────────────────────────────┘
                                                      │
                                           xray / sing-box / mihomo
```

- 新增项目 `v2rayN/v2rayN.Host`：ASP.NET Core minimal API，仅监听 `127.0.0.1` 随机端口，
  启动时生成一次性 token（stdout 或命名管道交给 Flutter），所有请求须带 `Authorization: Bearer`。
- Host 启动时设置 `AppManager.Instance.ShowInTaskbar = true`，否则日志不会 flush，统计事件也会被丢弃（见 AUDIT 第六节）。
- Host 实现 `IWindowDialog` 等 `Interaction<>` 的无界面版本：需要用户确认或选择文件时，
  通过 WebSocket 事件请求 Flutter 弹窗并等待响应。
- 新增目录 `flutter_ui/`：Flutter 客户端（Riverpod + go_router + `web_socket_channel` + `window_manager` + `tray_manager`）。
- Flutter 启动时拉起并守护 Host 进程；Host 在父进程退出后自行退出，并确保内核和系统代理被清理。

## 第一阶段范围（核心三件套）

审计发现 `ProfilesViewModel`、`MainWindowViewModel` 自带逐条派发、线性查找、全量刷新的问题（AUDIT P2、P3），
所以 Host **不复用这两个 ViewModel**，直接调用下层的 Manager 和 Handler，自己维护 `indexId → 行` 的映射。

| 页面 | Host 调用的 ServiceLib 部件 | API / 事件 |
|---|---|---|
| 节点列表（订阅分组、过滤、排序、测速） | `AppManager.ProfileModels`、`ConfigHandler`（排序、移动、设为默认）、`SpeedtestService`、`ProfileExManager` | `GET /profiles`、`POST /profiles/{id}/default`、`POST /profiles/test`；WS `profiles.delta`（只推变更行，测速结果 100ms 合并一次） |
| 状态栏 / 托盘 / 系统代理 / 路由切换 | `StatusBarViewModel`（无 UI 依赖的部分）、`SysProxyHandler`、`StatisticsManager` | `POST /sysproxy`、`POST /routing`；WS `status`（速度、运行节点） |
| 日志 | 订阅 `AppEvents.SendMsgViewRequested`（不经过 `MsgViewModel`） | WS `log`（环形缓冲，按批量推送，过滤在 Flutter 端做） |

之后阶段：服务器编辑、订阅设置、路由/DNS、选项设置、Clash 代理/连接页，以及下面的 fork 专属功能。

## 需要在 Flutter 里重做的 fork 功能
- LAN 共享卡片：纯逻辑在 `ServiceLib/Common/LanShare.cs`，可经 Host 复用；网卡枚举和防火墙操作在 `v2rayN.Desktop/Common/LanShareHelper.cs`，需要移到 Host。
- 右下角自动消失的 toast、托盘服务器和路由子菜单（`TrayMenuManager`）、启动和托盘显示不闪白（`WindowCloakHelper`）。
- UWP 回环结果提示（`UwpLoopbackHelper`）、设置项悬停说明（`ForkText.Tip*`）。
- TUN 记住上次选择、拒绝 UAC 后继续运行（ServiceLib 已实现，Host 只需透传结果）。

## 性能要点
1. 列表用 `ListView.builder` + 固定 itemExtent；WS 只推送 `{indexId, delay, speed}` 增量，Flutter 用 `ValueNotifier` 按行局部重建。
2. 日志环形缓冲（上限 5000 行），批量推送。
3. 测速结果节流（≥100ms 合并），避免大订阅时 UI 抖动。
4. 速度和流量图表用 `CustomPainter`，不引入重图表库。

## 里程碑
1. `v2rayN.Host` 骨架：启动 `AppManager.InitApp()`、鉴权、`/health`、WS 通道。在 `FORK_PATCHES.md` 登记一行。
2. 节点列表只读 + 增量推送 → Flutter 列表页，与 Avalonia 版（`v2rayN.Desktop`）对比渲染速度和内存。
3. 启停内核、系统代理、默认节点切换、测速。
4. 日志页、托盘、深浅色主题、i18n（`ResUI.*.resx` 和 `ForkText.cs` 的简中、繁中、英文字符串导出为 ARB）。
5. 打包：在 `build-windows-desktop.yml` / `package-zip.yml` 中新增独立产物，不改现有 `v2rayN-windows-64.zip` 等资产名（winget 依赖这些名字）。

## 风险
- **两套 Windows 前端并存**：fork 刚完成 Avalonia 重设计，过渡期内 Windows 上需要同时维护两套界面。建议设定明确的对齐清单，对齐后再决定 Avalonia Windows 包的去留。
- **提权路径**：`ProcUtils.RebootAsAdmin` 重启的是当前 exe（`Utils.GetExePath()`）。Host 模式下需要改为「以管理员身份重启 Flutter，再由它拉起 Host」，这部分需要 Host 自己实现，不能直接复用。
- `ServiceLib` 中的静态单例（`AppManager`、`StatusBarViewModel.Instance`）只允许单个宿主实例，符合预期；但 Host 与 Avalonia 版不能同时运行，要共用单实例互斥量（`Mutex "v2rayN"`）。
- `Interaction<>` 回调数量多，需逐个映射为 WS 请求和响应；第一阶段只实现用到的几个。
- AUDIT 中 ServiceLib 自身的 bug（如 H2、H3、F2）会同样影响 Flutter 版，需要按 fork 规则单独修复。
