# v2rayN (ds0chic fork)

A GUI client for Windows, Linux and macOS. Supports [Xray](https://github.com/XTLS/Xray-core) and [sing-box](https://github.com/SagerNet/sing-box) and [others](https://github.com/2dust/v2rayN/wiki/List-of-supported-cores).

---

## This is a personal fork / 个人分支

个人分支说明：

- 本仓库（ds0chic/v2rayN）由分支维护者独立维护，不向上游提交补丁。
- 上游项目为 [2dust/v2rayN](https://github.com/2dust/v2rayN)，原始作品版权归 2dust 及贡献者所有，许可证为 GPL-3.0。
- 上游修复同一问题后，本分支会移除对应补丁并采用上游的修复。
- `master` 是维护分支。同步方式：合并 `upstream/master`，详见 [FORK_PATCHES.md](FORK_PATCHES.md)。

This is a personal fork.

- This repository (ds0chic/v2rayN) is maintained independently by its owner. Patches are not submitted upstream.
- The upstream project is [2dust/v2rayN](https://github.com/2dust/v2rayN). The original work is copyright 2dust and contributors, licensed under GPL-3.0.
- When upstream fixes the same problem, the matching fork patch is dropped and the upstream fix is adopted.
- `master` is the maintained branch. Sync by merging `upstream/master`; see [FORK_PATCHES.md](FORK_PATCHES.md).

---

## What differs from upstream

The full patch registry, with the reason for each patch and the condition for dropping it, is in [FORK_PATCHES.md](FORK_PATCHES.md).

- **Avalonia-only front end.** The WPF front end is removed. One UI project (`v2rayN.Desktop`) is built for Windows, Linux and macOS.
- **Redesigned UI.** Two corner-radius tiers (10 for surfaces, 6 for controls). One menu button in the title-bar toolbar. A hover info marker on every setting. Light and dark themes only. Toasts appear bottom-right and dismiss themselves. The caption fullscreen button is hidden and the promotion entry is removed.
- **Startup and tray.** The window is cloaked and pre-warmed so tray start/show does not flash white. Single-instance wake-up.
- **TUN.** The last TUN choice is remembered across non-admin starts. Declining the UAC prompt keeps the app running. TUN is stopped before the core, and the stop/start order is corrected.
- **sing-box.** The stack is not forced to gvisor when unset. Sniffing is wired to sing-box's sniffer. QUIC is no longer sniffed by default.
- **LAN sharing card.** Dedicated port with generated credentials, status display, copyable addresses, and a firewall rule button.
- **Dual-stack routing.** Built-in white, black and global sets are IPv4+IPv6-aware and more precise. Unmodified old `V4-` sets are replaced by `V4V6-` automatically. Edited sets are merged, keeping the user's rules first.
- **UWP loopback.** Result feedback is shown as a toast.
- **Publish.** ReadyToRun is enabled for Release publish (larger single file, faster startup).
- **AI UI automation (test only).** A local named pipe for scripted UI tests. Opt-in with `V2RAYN_AI_AUTOMATION=1`, Windows only, intended for test instances only.

---

## Build / run

Requirements:

- .NET 10 SDK (CI uses `10.0.1xx`; `Directory.Build.props` targets `net10.0`).
- The `GlobalHotKeys` submodule (`v2rayN/GlobalHotKeys`, from `.gitmodules`). `v2rayN.Desktop` references it.

Clone and initialise the submodule:

```bash
git clone https://github.com/ds0chic/v2rayN.git
cd v2rayN
git submodule update --init --recursive
cd v2rayN
```

Build:

```bash
dotnet build v2rayN.Desktop/v2rayN.Desktop.csproj -c Release
```

Publish, as the CI workflows do (run from the inner `v2rayN` folder). Replace `<rid>` with one of `win-x64`, `win-arm64`, `win-x86`, `linux-x64`, `linux-arm64`, `osx-x64` or `osx-arm64`, and `<out>` with an output directory:

```bash
dotnet publish v2rayN.Desktop/v2rayN.Desktop.csproj -c Release -r <rid> -p:SelfContained=true -o <out>
```

For `win-*` RIDs, CI appends `-p:EnableWindowsTargeting=true` to the same command. Add it at the end when publishing Windows builds from a non-Windows machine:

```bash
dotnet publish v2rayN.Desktop/v2rayN.Desktop.csproj -c Release -r win-x64 -p:SelfContained=true -p:EnableWindowsTargeting=true -o <out>
```

Tests (same form as `test.yml`):

```bash
dotnet test --project ServiceLib.Tests -c Release
```

Release packages are built by the workflows in `.github/workflows` (`build-windows-desktop.yml`, `build-windows-x86.yml`, `build-linux.yml`, `build-osx.yml`). Bundled cores are downloaded from upstream's [v2rayN-core-bin](https://github.com/2dust/v2rayN-core-bin) repository.

---

## Supported platforms

| Platform | x64 | x86 | arm64 | riscv64 | loong64 |
| --- | --- | --- | --- | --- | --- |
| Windows | ✅ | ✅ | ✅ | - | - |
| Linux | ✅ | - | ✅ | ✅ | ✅ |
| macOS | ✅ | - | ✅ | - | - |

Platform matrix is taken from the build workflows in this repository.

---

## Documentation / 使用文档

The upstream Wiki covers usage and configuration. It describes upstream v2rayN and may not match the differences listed above.

上游 Wiki 包含使用说明和配置教程，内容基于上游版本，可能与本分支的差异不一致。

[https://github.com/2dust/v2rayN/wiki](https://github.com/2dust/v2rayN/wiki)

---

## License

Licensed under the GNU General Public License v3.0. See [LICENSE](LICENSE). The original work is copyright 2dust and contributors. Fork changes are distributed under the same license.

---

## Upstream and related projects / 上游与相关项目

- Upstream v2rayN: [https://github.com/2dust/v2rayN](https://github.com/2dust/v2rayN)
- Mobile client (v2rayNG): [https://github.com/2dust/v2rayNG](https://github.com/2dust/v2rayNG)
- Upstream Telegram group: [https://t.me/v2rayN](https://t.me/v2rayN)
- Upstream Telegram channel: [https://t.me/github_2dust](https://t.me/github_2dust)
