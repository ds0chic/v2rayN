using Avalonia.Threading;

namespace v2rayN.Desktop.Manager;

// NativeMenu has no ItemsSource, so the tray server and routing submenus are rebuilt in code.
internal static class TrayMenuManager
{
    private static readonly List<(ComboItem Item, NativeMenuItem MenuItem)> _serverMenuItems = [];
    private static readonly List<(RoutingItem Item, NativeMenuItem MenuItem)> _routingMenuItems = [];

    // Anchors are declared in App.axaml and located by header text, because x:Name is not generated on the Application root.
    public static void Attach(StatusBarViewModel vm, NativeMenu trayMenu)
    {
        var items = trayMenu.Items.OfType<NativeMenuItem>().ToList();
        var routingsMenuItem = items.FirstOrDefault(i => i.Header as string == ResUI.menuRouting);
        var serversMenuItem = items.FirstOrDefault(i => i.Header as string == ResUI.menuServers);
        var scanMenuItem = items.FirstOrDefault(i => i.Header as string == ResUI.menuAddServerViaScan);
        if (routingsMenuItem is null || serversMenuItem is null || scanMenuItem is null)
        {
            return;
        }

        scanMenuItem.IsVisible = Utils.IsWindows();

        var routingsMenu = new NativeMenu();
        var serversMenu = new NativeMenu();
        routingsMenuItem.Menu = routingsMenu;
        serversMenuItem.Menu = serversMenu;

        vm.Servers.CollectionChanged += (_, _) => Dispatcher.UIThread.Post(() => RebuildServers(vm, serversMenu));
        vm.RoutingItems.CollectionChanged += (_, _) => Dispatcher.UIThread.Post(() => RebuildRoutings(vm, routingsMenu));
        vm.WhenAnyValue(x => x.SelectedServer).Subscribe(_ => Dispatcher.UIThread.Post(() => UpdateServerChecks(vm)));
        vm.WhenAnyValue(x => x.SelectedRouting).Subscribe(_ => Dispatcher.UIThread.Post(() => UpdateRoutingChecks(vm)));

        RebuildServers(vm, serversMenu);
        RebuildRoutings(vm, routingsMenu);
    }

    private static void RebuildServers(StatusBarViewModel vm, NativeMenu menu)
    {
        menu.Items.Clear();
        _serverMenuItems.Clear();
        foreach (var server in vm.Servers)
        {
            var menuItem = new NativeMenuItem
            {
                Header = server.Text,
                ToggleType = MenuItemToggleType.Radio,
            };
            menuItem.Click += (_, _) => vm.SelectedServer = server;
            menu.Items.Add(menuItem);
            _serverMenuItems.Add((server, menuItem));
        }
        UpdateServerChecks(vm);
    }

    private static void RebuildRoutings(StatusBarViewModel vm, NativeMenu menu)
    {
        menu.Items.Clear();
        _routingMenuItems.Clear();
        foreach (var routing in vm.RoutingItems)
        {
            var menuItem = new NativeMenuItem
            {
                Header = routing.Remarks,
                ToggleType = MenuItemToggleType.Radio,
            };
            menuItem.Click += (_, _) => vm.SelectedRouting = routing;
            menu.Items.Add(menuItem);
            _routingMenuItems.Add((routing, menuItem));
        }
        UpdateRoutingChecks(vm);
    }

    private static void UpdateServerChecks(StatusBarViewModel vm)
    {
        var selectedId = vm.SelectedServer?.ID;
        foreach (var (item, menuItem) in _serverMenuItems)
        {
            menuItem.IsChecked = selectedId.IsNotEmpty() && item.ID == selectedId;
        }
    }

    private static void UpdateRoutingChecks(StatusBarViewModel vm)
    {
        var selectedId = vm.SelectedRouting?.Id;
        foreach (var (item, menuItem) in _routingMenuItems)
        {
            menuItem.IsChecked = selectedId.IsNotEmpty() && item.Id == selectedId;
        }
    }
}
