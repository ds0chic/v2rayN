using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using System.Text.Json.Nodes;

namespace v2rayN.Desktop.Common;

// Command implementations for AiAutomation. Everything that touches controls runs on the UI thread.
internal static class AiAutomationCommands
{
    private const int MaxTreeNodes = 2000;
    private const int DefaultTreeDepth = 12;
    private const int UiTimeoutSeconds = 10;
    private const int MaxWaitMs = 10000;

    // Click/set refuse controls whose x:Name contains any of these (case-insensitive).
    private static readonly string[] DeniedNameParts =
    [
        "firewall", "rebootasadmin", "uwp", "sudo", "pass", "startboot", "autorun"
    ];

    // Text of these controls is never echoed; they are masked as "***".
    private static readonly string[] MaskedNameParts = ["pass", "user"];

    public static async Task<(JsonObject Response, bool ExitAfterReply)> ExecuteAsync(string line)
    {
        List<string> tokens;
        try
        {
            tokens = Tokenize(line);
        }
        catch (FormatException ex)
        {
            return (Error(ex.Message), false);
        }

        if (tokens.Count == 0)
        {
            return (Error("empty command"), false);
        }

        var cmd = tokens[0].ToLowerInvariant();
        Logging.SaveLog($"AiAutomation command: {cmd}");

        switch (cmd)
        {
            case "wait":
                return (await WaitAsync(tokens), false);
            case "exit":
                return (Ok(new JsonObject { ["exiting"] = true }), true);
        }

        try
        {
            var completion = new TaskCompletionSource<JsonObject>(TaskCreationOptions.RunContinuationsAsynchronously);
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    completion.TrySetResult(Dispatch(cmd, tokens));
                }
                catch (Exception ex)
                {
                    completion.TrySetException(ex);
                }
            });

            var finished = await Task.WhenAny(completion.Task, Task.Delay(TimeSpan.FromSeconds(UiTimeoutSeconds)));
            if (finished != completion.Task)
            {
                return (Error($"UI thread did not respond within {UiTimeoutSeconds} seconds"), false);
            }

            return (await completion.Task, false);
        }
        catch (Exception ex)
        {
            return (Error(ex.Message), false);
        }
    }

    private static JsonObject Dispatch(string cmd, List<string> tokens)
    {
        return cmd switch
        {
            "windows" => ListWindows(),
            "tree" => Tree(tokens),
            "click" => Click(tokens),
            "set" => Set(tokens),
            "text" => Text(tokens),
            "shot" => Shot(tokens),
            "close" => Close(tokens),
            _ => Error($"unknown command: {cmd}"),
        };
    }

    private static async Task<JsonObject> WaitAsync(List<string> tokens)
    {
        if (tokens.Count != 2 || !int.TryParse(tokens[1], out var ms) || ms < 0 || ms > MaxWaitMs)
        {
            return Error($"usage: wait <milliseconds 0-{MaxWaitMs}>");
        }

        await Task.Delay(ms);
        return Ok();
    }

    #region Commands

    private static JsonObject ListWindows()
    {
        var lifetime = Lifetime();
        if (lifetime is null)
        {
            return Error("not a classic desktop application");
        }

        var list = new JsonArray();
        foreach (var w in lifetime.Windows)
        {
            list.Add(new JsonObject
            {
                ["title"] = w.Title ?? string.Empty,
                ["type"] = w.GetType().Name,
                ["width"] = w.Bounds.Width,
                ["height"] = w.Bounds.Height,
                ["visible"] = w.IsVisible,
                ["active"] = w.IsActive,
                ["main"] = ReferenceEquals(w, lifetime.MainWindow),
            });
        }

        return Ok(new JsonObject { ["windows"] = list });
    }

    private static JsonObject Tree(List<string> tokens)
    {
        var args = tokens.Skip(1).ToList();
        var depth = DefaultTreeDepth;
        if (args.Count > 0 && int.TryParse(args[^1], out var parsedDepth))
        {
            depth = Math.Clamp(parsedDepth, 1, 50);
            args.RemoveAt(args.Count - 1);
        }

        var window = FindWindow(string.Join(' ', args));
        if (window is null)
        {
            return Error("window not found");
        }

        var budget = new TreeBudget();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var root = BuildNode(window, 0, depth, budget, visited);
        return Ok(new JsonObject
        {
            ["window"] = window.Title ?? string.Empty,
            ["nodeCount"] = budget.Count,
            ["truncated"] = budget.Truncated,
            ["tree"] = root,
        });
    }

    private static JsonObject Click(List<string> tokens)
    {
        if (tokens.Count != 2)
        {
            return Error("usage: click <x:Name>");
        }

        var name = tokens[1];
        if (IsDeniedName(name))
        {
            return Error($"click refused: '{name}' is blocked by the automation safety list");
        }

        var control = FindControl(name);
        if (control is null)
        {
            return Error($"control not found: {name}");
        }

        if (!control.IsEffectivelyEnabled)
        {
            return Error($"control is disabled: {name}");
        }

        switch (control)
        {
            case ToggleButton toggle:
                toggle.IsChecked = toggle.IsChecked != true;
                break;
            case Button button:
                ClickCommandSource(button, Button.ClickEvent, button);
                break;
            case MenuItem menuItem:
                ClickCommandSource(menuItem, MenuItem.ClickEvent, menuItem);
                break;
            case TabItem tabItem:
                tabItem.IsSelected = true;
                break;
            case ComboBoxItem comboItem:
                comboItem.IsSelected = true;
                break;
            default:
                return Error($"click not supported for {control.GetType().Name}: {name}");
        }

        return Ok(new JsonObject { ["name"] = name, ["type"] = control.GetType().Name });
    }

    private static JsonObject Set(List<string> tokens)
    {
        if (tokens.Count < 3)
        {
            return Error("usage: set <x:Name> <value>");
        }

        var name = tokens[1];
        if (IsDeniedName(name))
        {
            return Error($"set refused: '{name}' is blocked by the automation safety list");
        }

        var value = string.Join(' ', tokens.Skip(2));
        var control = FindControl(name);
        if (control is null)
        {
            return Error($"control not found: {name}");
        }

        if (!control.IsEffectivelyEnabled)
        {
            return Error($"control is disabled: {name}");
        }

        switch (control)
        {
            case TextBox textBox:
                textBox.Text = value;
                break;
            case ToggleButton toggle:
                if (!TryParseBool(value, out var checkedValue))
                {
                    return Error("value for a toggle must be true/false (or 1/0, on/off, yes/no)");
                }

                toggle.IsChecked = checkedValue;
                break;
            case ComboBox comboBox:
                if (!TrySelectByTextOrIndex(comboBox.Items, value, out var comboIndex))
                {
                    return Error($"no ComboBox item matches '{value}' (by text or index)");
                }

                comboBox.SelectedIndex = comboIndex;
                break;
            case TabControl tabControl:
                if (!TrySelectByTextOrIndex(tabControl.Items, value, out var tabIndex))
                {
                    return Error($"no TabItem matches '{value}' (by header text or index)");
                }

                tabControl.SelectedIndex = tabIndex;
                break;
            default:
                return Error($"set not supported for {control.GetType().Name}: {name}");
        }

        return Ok(new JsonObject { ["name"] = name, ["type"] = control.GetType().Name });
    }

    private static JsonObject Text(List<string> tokens)
    {
        if (tokens.Count != 2)
        {
            return Error("usage: text <x:Name>");
        }

        var control = FindControl(tokens[1]);
        if (control is null)
        {
            return Error($"control not found: {tokens[1]}");
        }

        var node = Describe(control);
        node.Insert(0, "ok", JsonValue.Create(true));
        return node;
    }

    private static JsonObject Shot(List<string> tokens)
    {
        if (tokens.Count < 2)
        {
            return Error("usage: shot <output.png> [window title text]");
        }

        var path = tokens[1];
        if (!string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase))
        {
            return Error("shot output must be a .png file");
        }

        var window = FindWindow(string.Join(' ', tokens.Skip(2)));
        if (window is null)
        {
            return Error("window not found");
        }

        if (!window.IsVisible)
        {
            return Error("window is hidden; nothing to render");
        }

        var scale = window.RenderScaling;
        var widthDip = window.ClientSize.Width;
        var heightDip = window.ClientSize.Height;
        if (!double.IsFinite(widthDip) || !double.IsFinite(heightDip) || widthDip <= 0 || heightDip <= 0 || !double.IsFinite(scale) || scale <= 0)
        {
            return Error("window has no measured size yet");
        }

        var pixelWidth = (int)Math.Round(widthDip * scale);
        var pixelHeight = (int)Math.Round(heightDip * scale);
        var fullPath = Path.GetFullPath(path);

        using var bitmap = new RenderTargetBitmap(new PixelSize(pixelWidth, pixelHeight), new Vector(96 * scale, 96 * scale));
        bitmap.Render(window);

        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        bitmap.Save(fullPath, new PngBitmapEncoderOptions());
        return Ok(new JsonObject
        {
            ["path"] = fullPath,
            ["width"] = pixelWidth,
            ["height"] = pixelHeight,
        });
    }

    private static JsonObject Close(List<string> tokens)
    {
        var title = string.Join(' ', tokens.Skip(1));
        var lifetime = Lifetime();
        if (lifetime is null)
        {
            return Error("not a classic desktop application");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return Error("usage: close <window title text> (the main window cannot be closed)");
        }

        var target = lifetime.Windows.FirstOrDefault(w => !ReferenceEquals(w, lifetime.MainWindow)
                                                          && (w.Title ?? string.Empty).Contains(title, StringComparison.OrdinalIgnoreCase));
        if (target is null)
        {
            return Error("window not found (the main window cannot be closed)");
        }

        target.Close();
        return Ok(new JsonObject { ["closed"] = target.Title ?? string.Empty });
    }

    #endregion Commands

    #region Click helpers

    private static void ClickCommandSource(Control control, RoutedEvent<RoutedEventArgs> clickEvent, ICommandSource source)
    {
        var args = new RoutedEventArgs(clickEvent);
        control.RaiseEvent(args);
        if (args.Handled || source.Command is null)
        {
            return;
        }

        if (source.Command.CanExecute(source.CommandParameter))
        {
            source.Command.Execute(source.CommandParameter);
        }
    }

    private static bool TryParseBool(string value, out bool result)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "true":
            case "1":
            case "on":
            case "yes":
                result = true;
                return true;
            case "false":
            case "0":
            case "off":
            case "no":
                result = false;
                return true;
            default:
                result = false;
                return false;
        }
    }

    // Exact text match wins; a plain integer falls back to an index.
    private static bool TrySelectByTextOrIndex(IEnumerable<object?> items, string value, out int index)
    {
        var list = items.ToList();
        for (var i = 0; i < list.Count; i++)
        {
            if (string.Equals(DisplayText(list[i]), value, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                return true;
            }
        }

        if (int.TryParse(value, out var parsed) && parsed >= 0 && parsed < list.Count)
        {
            index = parsed;
            return true;
        }

        index = -1;
        return false;
    }

    #endregion Click helpers

    #region Tree and state

    private sealed class TreeBudget
    {
        public int Count { get; set; }
        public bool Truncated { get; set; }
    }

    private static JsonObject BuildNode(Control control, int level, int maxDepth, TreeBudget budget, HashSet<object> visited)
    {
        budget.Count++;
        var node = Describe(control);
        if (level >= maxDepth)
        {
            return node;
        }

        var children = new JsonArray();
        foreach (var child in ChildrenOf(control))
        {
            if (!visited.Add(child))
            {
                continue;
            }

            if (budget.Count >= MaxTreeNodes)
            {
                budget.Truncated = true;
                break;
            }

            children.Add(BuildNode(child, level + 1, maxDepth, budget, visited));
        }

        if (children.Count > 0)
        {
            node["children"] = children;
        }

        return node;
    }

    // Common description used by tree and text.
    private static JsonObject Describe(Control control)
    {
        var node = new JsonObject
        {
            ["type"] = control.GetType().Name,
            ["name"] = string.IsNullOrEmpty(control.Name) ? null : control.Name,
            ["visible"] = control.IsVisible,
            ["enabled"] = control.IsEnabled,
        };

        var masked = IsMaskedControl(control);
        switch (control)
        {
            case TextBox textBox:
                node["text"] = masked ? "***" : textBox.Text ?? string.Empty;
                break;
            case TextBlock textBlock:
                node["text"] = masked ? "***" : textBlock.Text ?? string.Empty;
                break;
            case ToggleButton toggle:
                node["checked"] = toggle.IsChecked == true;
                node["text"] = MaskIfNeeded(DisplayText(toggle.Content), masked);
                break;
            case ComboBox comboBox:
                node["selectedIndex"] = comboBox.SelectedIndex;
                node["text"] = MaskIfNeeded(DisplayText(comboBox.SelectedItem), masked);
                break;
            case TabControl tabControl:
                node["selectedIndex"] = tabControl.SelectedIndex;
                node["text"] = MaskIfNeeded(DisplayText(tabControl.SelectedItem), masked);
                break;
            case Expander expander:
                node["expanded"] = expander.IsExpanded;
                node["text"] = MaskIfNeeded(DisplayText(expander.Header, deep: true), masked);
                break;
            case TabItem tabItem:
                node["selected"] = tabItem.IsSelected;
                node["text"] = MaskIfNeeded(DisplayText(tabItem.Header), masked);
                break;
            case ComboBoxItem comboItem:
                node["selected"] = comboItem.IsSelected;
                node["text"] = MaskIfNeeded(DisplayText(comboItem.Content), masked);
                break;
            case MenuItem menuItem:
                node["text"] = MaskIfNeeded(DisplayText(menuItem.Header), masked);
                break;
            case ContentControl contentControl:
                // Button content may be a panel of icon + text; other containers are not joined to avoid repeating subtrees.
                var text = DisplayText(contentControl.Content, deep: control is Button);
                if (text is not null)
                {
                    node["text"] = MaskIfNeeded(text, masked);
                }

                break;
        }

        return node;
    }

    private static string? MaskIfNeeded(string? text, bool masked)
    {
        return text is null ? null : masked ? "***" : text;
    }

    // Best-effort plain text for a header/content/item object.
    private static string? DisplayText(object? item, bool deep = false)
    {
        switch (item)
        {
            case null:
                return null;
            case string s:
                return s;
            case TextBlock tb:
                return tb.Text;
            case ComboBoxItem cbi:
                return DisplayText(cbi.Content);
            case TabItem ti:
                return DisplayText(ti.Header);
            case ContentControl cc:
                return DisplayText(cc.Content);
            case Panel panel when deep:
                var parts = panel.Children.Select(child => DisplayText(child, deep: true)).Where(p => !string.IsNullOrEmpty(p)).ToArray();
                return parts.Length == 0 ? null : string.Join(' ', parts);
            default:
                return null;
        }
    }

    private static bool IsMaskedControl(Control control)
    {
        if (control is TextBox textBox && textBox.PasswordChar != '\0')
        {
            return true;
        }

        var name = control.Name ?? string.Empty;
        return MaskedNameParts.Any(part => name.Contains(part, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsDeniedName(string name)
    {
        return DeniedNameParts.Any(part => name.Contains(part, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<Control> ChildrenOf(Control control)
    {
        foreach (var logical in control.GetLogicalChildren())
        {
            if (logical is Control c)
            {
                yield return c;
            }
        }

        if (control is ItemsControl items)
        {
            foreach (var item in items.Items)
            {
                if (item is Control c)
                {
                    yield return c;
                }
            }
        }

        if (control is ContentControl content && content.Content is Control contentControl)
        {
            yield return contentControl;
        }

        if (control is Decorator decorator && decorator.Child is Control child)
        {
            yield return child;
        }
    }

    #endregion Tree and state

    #region Lookup

    private static IClassicDesktopStyleApplicationLifetime? Lifetime()
    {
        return Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
    }

    // Empty filter means the main window; otherwise the first window whose title contains the text.
    private static Window? FindWindow(string titleFilter)
    {
        var lifetime = Lifetime();
        if (lifetime is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(titleFilter))
        {
            return lifetime.MainWindow;
        }

        return lifetime.Windows.FirstOrDefault(w => (w.Title ?? string.Empty).Contains(titleFilter, StringComparison.OrdinalIgnoreCase));
    }

    // Searches all windows (main window first) through the logical tree plus item/content children.
    private static Control? FindControl(string name)
    {
        var lifetime = Lifetime();
        if (lifetime is null)
        {
            return null;
        }

        var roots = new List<Window>();
        if (lifetime.MainWindow is { } main)
        {
            roots.Add(main);
        }

        roots.AddRange(lifetime.Windows.Where(w => !ReferenceEquals(w, lifetime.MainWindow)));

        foreach (var root in roots)
        {
            var found = WalkAll(root).FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    private static IEnumerable<Control> WalkAll(Control root)
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<Control>();
        stack.Push(root);
        visited.Add(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            foreach (var child in ChildrenOf(current).Reverse())
            {
                if (visited.Add(child))
                {
                    stack.Push(child);
                }
            }
        }
    }

    #endregion Lookup

    #region Parsing and responses

    // Splits on whitespace; double quotes group words. Backslashes are literal (Windows paths).
    private static List<string> Tokenize(string line)
    {
        var result = new List<string>();
        var sb = new System.Text.StringBuilder();
        var inQuotes = false;
        var hasToken = false;

        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                hasToken = true;
                continue;
            }

            if (!inQuotes && char.IsWhiteSpace(ch))
            {
                if (hasToken)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                    hasToken = false;
                }

                continue;
            }

            sb.Append(ch);
            hasToken = true;
        }

        if (inQuotes)
        {
            throw new FormatException("unterminated double quote");
        }

        if (hasToken)
        {
            result.Add(sb.ToString());
        }

        return result;
    }

    private static JsonObject Ok(JsonObject? body = null)
    {
        body ??= new JsonObject();
        body.Insert(0, "ok", JsonValue.Create(true));
        return body;
    }

    private static JsonObject Error(string message)
    {
        return new JsonObject
        {
            ["ok"] = false,
            ["error"] = message,
        };
    }

    #endregion Parsing and responses
}
