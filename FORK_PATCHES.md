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
| Settings hints | options that only apply to one core carry a small badge; per-option hints are hover info markers (no inline text) | OptionSettingWindow.axaml, ForkText.cs | hides or documents these options itself |
| Startup and tray | window cloak + pre-warm so tray start/show never flashes white or shows an empty frame; startup timing log; single-instance wake | MainWindow.axaml.cs, WindowCloakHelper.cs, App.axaml.cs | fixes the tray flash itself |
| Avalonia parity | tray server/routing submenus and scan, close-to-tray, dark title bar, hardware acceleration option, auto-adjust column width, config-load error dialog | MainWindow.axaml(.cs), App.axaml, Program.cs, tray code | adds the same features |
| LAN sharing card | one-click safe LAN sharing (dedicated port + generated credentials), copyable proxy addresses, firewall rule button (delete then add) | LanShareCard.axaml(.cs), LanShareHelper.cs, ServiceLib/Common/LanShare.cs (+ tests) | offers equivalent LAN sharing UI |
| Redesigned views | see "Redesigned views" below | many .axaml | n/a |
| Dead-code cleanup | removal of unused members/resources/commented code (ResUI key removal was reverted to limit merge conflicts) | many | n/a (resolve conflicts in favor of upstream) |
| Avalonia-only frontend | WPF frontend removed; only v2rayN.Desktop is built. Deleted: `v2rayN/v2rayN/**`, WPF-only packages (H.NotifyIcon.Wpf, MaterialDesignThemes, ReactiveUI.WPF) in Directory.Packages.props, sln/slnx entries, `*_wpftmp.csproj` ignore rule, `build-windows.yml` (and its dispatch in build-all.yml). CI asset names kept: Avalonia Windows builds use package-zip.yml target `windows`, producing `v2rayN-windows-64.zip` / `v2rayN-windows-arm64.zip` (winget-publish.yml unchanged) and x86 `v2rayN-windows-86.zip` | v2rayN/v2rayN/** (deleted), sln/slnx, Directory.Packages.props, .gitignore, workflows | n/a; on modify/delete conflicts in v2rayN/v2rayN/** run `git rm` (keep it deleted) |

## ForkText

- `v2rayN/v2rayN.Desktop/Common/ForkText.cs` holds fork-only UI strings as static properties, selected by UI culture.
- New UI text added by this fork goes there, never into `ResUI.resx` (or the generated `ResUI.Designer.cs`), so upstream resource merges stay conflict-free.
- Languages: Simplified Chinese (zh-Hans), Traditional Chinese (zh-Hant, used for zh-TW/zh-HK/zh-MO), and English as the fallback for every other language. Use the existing `Pick(hans, hant, en)` pattern.

## AI UI automation

Opt-in test-only pipe that lets scripts open windows, click controls, read text and take screenshots. Lives in new files; the only upstream-file change is one line in `App.axaml.cs` (`AiAutomation.Start();` after `Dispatcher.UIThread.Post(StartupTiming.Flush, ...)`).

- Files: `v2rayN/v2rayN.Desktop/Common/AiAutomation.cs` (pipe host, enable check), `v2rayN/v2rayN.Desktop/Common/AiAutomationCommands.cs` (commands), `tools/ai-ui.ps1` (client).
- Enable: start the app with environment variable `V2RAYN_AI_AUTOMATION=1`. Without it, `Start()` returns immediately and no thread or pipe is created. Windows only; other platforms return without effect.
- Pipe: `v2rayN-ai-<process id>`, local named pipe only (no network listener). Access is granted to the current Windows user SID; the network SID is denied.
- Protocol: one command per line, UTF-8. Each response is one JSON line: `{"ok":true,...}` or `{"ok":false,"error":"..."}`. Every control access runs on the UI thread (with a 10 second timeout).
- Commands:
  - `windows`: list open windows (title, type, width, height, visible, active, main).
  - `tree [title text] [depth]`: logical tree of a window (plus item and content children) (default main window, default depth 12, at most 2000 nodes). Each node has type, name, visible, enabled, and text/checked/selectedIndex/selected where applicable.
  - `click <x:Name>`: Button and MenuItem raise Click, then run Command if the event was not handled. ToggleButton (ToggleSwitch/CheckBox) toggles. TabItem and ComboBoxItem are selected.
  - `set <x:Name> <value>`: TextBox sets text; toggles take true/false/1/0/on/off/yes/no; ComboBox and TabControl match item text first, then a plain integer index.
  - `text <x:Name>`: current state of a control (same fields as a tree node, `ok` added).
  - `shot <output.png> [title text]`: renders a visible window at its render scaling to PNG (creates the directory). Returns pixel size.
  - `close <title text>`: closes a non-main window. The main window cannot be closed.
  - `wait <ms>`: waits 0 to 10000 ms on the pipe thread.
  - `exit`: replies, then `Environment.Exit(0)`. Bypasses `AppExitAsync`, so cores and the tray icon may not be cleaned up; use only on test instances.
- Tokenizing: whitespace separates tokens; double quotes group words. Backslashes are literal. `set` joins the remaining tokens with single spaces.
- Safety:
  - `click` and `set` refuse names containing (case-insensitive) `firewall`, `rebootasadmin`, `uwp`, `sudo`, `pass`, `startboot`, `autorun`. `pass` is broader than the spec's `password` because `txtpass` is a real control name; `autorun` covers the start-on-boot toggle `togAutoRun`.
  - `tree` and `text` print `***` for any TextBox with a PasswordChar, and for controls whose name contains `pass` or `user`.
  - Command names are logged through `Logging.SaveLog`; set values are not logged.
- Known limits: popups and menus opened as separate top-level windows are not in `shot`. `exit` skips normal shutdown. `set` joins multi-space values into single spaces.
- Client: `powershell -NoProfile -ExecutionPolicy Bypass -File tools\ai-ui.ps1 -ProcessId <pid> -Command "windows"`. It connects with a 5 second deadline, prints one JSON line, and exits 1 on failure. Compatible with Windows PowerShell 5.1.
- Convention: use this only on test instances (for example a copy under a scratch directory with its own config), never on the instance you use day to day.

## Redesigned views

The Avalonia view layer was restyled with a fork design system (`v2rayN.Desktop/Assets/ForkTheme.axaml`, colors come from Semi tokens so theme variants keep working). Element names (`x:Name`), bindings and commands were kept so code-behind and view models are unchanged.

- Design rules: two corner-radius tiers only (surfaces 10, controls 6); cards (`Border.card`, `Border.settingsGroup`); text roles (`title`, `secondary`, `sectionTitle`); hints are hover info markers (`Border.infoDot`), never inline paragraphs; quiet motion (one easing curve, short color fades, tiny press-in, page cross-fade, 90 ms window fade-in).
- Rewritten or restructured views: `MainWindow` (unified title-bar toolbar with a single menu button; all former menus live inside it), `ProfilesView`, `MsgView`, `StatusBarView`, `OptionSettingWindow` (10 pages, left navigation), `LanShareCard`, `AddServerWindow` (sections become cards that follow the visibility of the named section the code-behind toggles).
- Shared styles: `Assets/GlobalStyles.axaml` (confirm button `Button#btnSave` accent) and `Assets/ForkTheme.axaml`.
- Sync rule: when upstream edits one of the views above, keep OUR file and port the new upstream controls by hand. Then check the `x:Name` set against upstream's version of the file (every upstream name must still exist) and that new bindings are present. The AI UI automation tool below makes the visual check cheap.
- Title bar: the toolbar draws its own title, so `ForkTheme.axaml` fades Semi's drawn title (`PART_TitleTextPanel`, Opacity because the template binds IsVisible). Re-check the name after Semi upgrades; the toolbar must not set a Background (it would swallow title-bar drag).
- Do not remove named elements the code-behind toggles (for example `sepa2` in `AddServerWindow` is kept at opacity 0).

## Sync checklist
1. `git fetch upstream`
2. `git log --oneline HEAD..upstream/master` and read it for fixes overlapping the table above.
3. Drop patches that upstream now covers; rebase the rest.
4. Build `v2rayN.Desktop`, run `dotnet test ServiceLib.Tests`.
