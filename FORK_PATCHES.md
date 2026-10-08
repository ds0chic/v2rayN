# Fork patch registry

This fork (ds0chic/v2rayN) is maintained independently and is not submitted upstream.
Upstream: https://github.com/2dust/v2rayN (remote `upstream`).

## Sync policy
- Sync by rebasing the fork patches onto `upstream/master` (`git rerere` is enabled).
- Keep every patch small and as isolated as possible; prefer new files over editing upstream files.
- When upstream fixes the same problem, drop the fork patch and adopt the upstream fix.
  Check the "Drop when" column of each patch below on every sync.

## Patches

| Patch | Intent | Main files | Drop when upstream... |
|---|---|---|---|
| TUN remembers last choice | `TunModeItem.LastEnableTun`; non-admin start no longer discards the user's TUN choice | ConfigItems.cs, StatusBarViewModel.cs | persists the TUN choice across non-admin starts |
| UAC decline keeps app running | `RebootAsAdmin` returns bool; TUN restored after elevated restart | AppManager.cs, StatusBarViewModel.cs | stops exiting on UAC decline |
| sing-box stack and sniffer | do not force gvisor when stack unset; sniffing type wired to sing-box sniffer (dns always included); QUIC no longer sniffed by default | SingboxInboundService.cs, SingboxRoutingService.cs | maps sniffing type to sing-box sniffer itself |
| TUN start/stop order | stop TUN before core; remove Wintun device and confirm; no hard TUN start on proxy port timeout | CoreManager.cs, WindowsUtils.cs | reworks TUN stop/start sequencing |
| ReadyToRun | `PublishReadyToRun=true` (bigger single file, faster startup) | Directory.Build.props | enables ReadyToRun itself |
| Settings tooltips | marks options that have no effect under sing-box / Xray native TUN | OptionSettingWindow.axaml, ResUI.resx | hides or documents these options itself |
| Dead-code cleanup | removal of unused members/resources/commented code (ResUI key removal was reverted to limit merge conflicts) | many | n/a (resolve conflicts in favor of upstream) |
| Avalonia-only frontend | WPF frontend removed; only v2rayN.Desktop is built. Deleted: `v2rayN/v2rayN/**`, WPF-only packages (H.NotifyIcon.Wpf, MaterialDesignThemes, ReactiveUI.WPF) in Directory.Packages.props, sln/slnx entries, `*_wpftmp.csproj` ignore rule, `build-windows.yml` (and its dispatch in build-all.yml). CI asset names kept: Avalonia Windows builds use package-zip.yml target `windows`, producing `v2rayN-windows-64.zip` / `v2rayN-windows-arm64.zip` (winget-publish.yml unchanged) and x86 `v2rayN-windows-86.zip` | v2rayN/v2rayN/** (deleted), sln/slnx, Directory.Packages.props, .gitignore, workflows | n/a; on modify/delete conflicts in v2rayN/v2rayN/** run `git rm` (keep it deleted) |

## Sync checklist
1. `git fetch upstream`
2. `git log --oneline HEAD..upstream/master` and read it for fixes overlapping the table above.
3. Drop patches that upstream now covers; rebase the rest.
4. Build `v2rayN.Desktop`, run `dotnet test ServiceLib.Tests`.
