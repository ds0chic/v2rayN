# v2rayN Flutter 前端重构方案（Windows，第一阶段）

## 目标
- 性能：节点列表虚拟化渲染、测速结果增量刷新、日志流不卡 UI。
- 体验：现代、轻量、美观的界面（Material 3 / Fluent 风格，深浅色）。
- 可维护：**不修改 `ServiceLib` 任何文件**，跟随官方上游更新时只需 merge，无冲突。

## 架构（方案 A：本地 IPC 后端）

```
┌────────────────────┐   loopback + token    ┌───────────────────────────┐
│ Flutter (Windows)  │ ◄───────────────────► │ v2rayN.Host (.NET, headless)│
│ UI only            │  REST + WebSocket     │ 引用 ServiceLib（原样复用）  │
└────────────────────┘                       └───────────────────────────┘
                                                      │
                                           xray / sing-box / mihomo
```

- 新增项目 `v2rayN/v2rayN.Host`：ASP.NET Core minimal API，仅监听 `127.0.0.1` 随机端口，
  启动时生成一次性 token（stdout 或命名管道交给 Flutter），所有请求须带 `Authorization: Bearer`。
- Host 实现 `IWindowDialog` 等 `Interaction<>` 的无界面版本：需要用户确认/文件选择时，
  通过 WebSocket 事件请求 Flutter 弹窗并等待响应。
- 新增目录 `flutter_ui/`：Flutter 客户端（Riverpod + go_router + `web_socket_channel` + `window_manager` + `tray_manager`）。
- Flutter 启动时拉起并守护 Host 进程；Host 在父进程退出后自行退出并确保内核/系统代理被清理。

## 第一阶段范围（核心三件套）
| 页面 | 复用的 ViewModel | API/事件 |
|---|---|---|
| 节点列表（含订阅分组、过滤、排序、测速） | `ProfilesViewModel` | `GET /profiles`, `POST /profiles/{id}/default`, `POST /profiles/test`；WS: `profiles.delta`（只推变更行） |
| 状态栏 / 托盘 / 系统代理 / 路由切换 | `StatusBarViewModel` | `POST /sysproxy`, `POST /routing`；WS: `status`（速度、运行节点） |
| 日志 | `MsgViewModel` | WS: `log`（批量合并，每 100ms 一批） |

之后阶段：服务器编辑、订阅设置、路由/DNS、选项设置、Clash 代理/连接页。

## 性能要点
1. 列表用 `ListView.builder` + 固定 itemExtent；WS 只推送 `{indexId, delay, speed}` 增量，Flutter 用 `ValueNotifier` 按行局部重建。
2. 日志环形缓冲（上限 5000 行），批量推送。
3. 测速结果节流（≥100ms 合并），避免大订阅时 UI 抖动。
4. 速度/流量图表用 `CustomPainter`，不引入重图表库。

## 里程碑
1. `v2rayN.Host` 骨架：启动 `AppManager.InitApp()`、鉴权、`/health`、WS 通道。
2. 节点列表只读 + 增量推送 → Flutter 列表页，与 Avalonia 版做渲染/内存对比。
3. 启停内核、系统代理、默认节点切换、测速。
4. 日志页、托盘、深浅色主题、i18n（复用 `ResUI.*.resx` 导出为 ARB）。
5. 打包：Host + Flutter 一起放进现有 `build-windows-desktop.yml` 流程。

## 风险
- `ServiceLib` 中静态单例（`AppManager`、`StatusBarViewModel.Instance`）仅允许单宿主实例——符合预期。
- `Interaction<>` 回调数量多，需逐个映射为 WS 请求/响应；第一阶段只实现用到的几个。
- 提权（TUN / 管理员）流程需沿用现有 `CoreAdminManager`，Host 以同一权限启动。
