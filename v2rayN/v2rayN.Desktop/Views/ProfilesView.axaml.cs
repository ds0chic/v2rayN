using Avalonia.VisualTree;
using DialogHostAvalonia;
using v2rayN.Desktop.Common;

namespace v2rayN.Desktop.Views;

public partial class ProfilesView : ReactiveUserControl<ProfilesViewModel>
{
    private static Config _config;
    private static readonly string _tag = "ProfilesView";

    public ProfilesView()
    {
        InitializeComponent();

        _config = AppManager.Instance.Config;

        menuSelectAll.Click += menuSelectAll_Click;
        btnAutofitColumnWidth.Click += BtnAutofitColumnWidth_Click;
        menuShowTrafficColumns.Click += MenuShowTrafficColumns_Click;
        txtServerFilter.KeyDown += TxtServerFilter_KeyDown;
        lstProfiles.KeyDown += LstProfiles_KeyDown;
        lstProfiles.SelectionChanged += lstProfiles_SelectionChanged;
        lstProfiles.DoubleTapped += LstProfiles_DoubleTapped;
        lstProfiles.LoadingRow += LstProfiles_LoadingRow;
        lstProfiles.Sorting += LstProfiles_Sorting;

        // fork: press on a row and drag up or down to select the rows in between
        lstProfiles.AddHandler(PointerPressedEvent, LstProfiles_DragSelectPressed, RoutingStrategies.Tunnel, true);
        lstProfiles.AddHandler(PointerMovedEvent, LstProfiles_DragSelectMoved, RoutingStrategies.Bubble, true);
        lstProfiles.AddHandler(PointerReleasedEvent, LstProfiles_DragSelectReleased, RoutingStrategies.Bubble, true);
        lstProfiles.PointerCaptureLost += LstProfiles_DragSelectCaptureLost;

        if (_config.UiItem.EnableDragDropSort)
        {
            lstProfiles.SetValue(DragDrop.AllowDropProperty, true);

            lstProfiles.AddHandler(PointerPressedEvent, LstProfiles_PointerPressed, RoutingStrategies.Bubble, true);
            lstProfiles.AddHandler(PointerMovedEvent, LstProfiles_PointerMoved, RoutingStrategies.Bubble, true);
            lstProfiles.AddHandler(PointerReleasedEvent, LstProfiles_PointerReleased, RoutingStrategies.Bubble, true);
            lstProfiles.AddHandler(DragDrop.DragOverEvent, LstProfiles_DragOver, RoutingStrategies.Bubble);
            lstProfiles.AddHandler(DragDrop.DropEvent, LstProfiles_Drop, RoutingStrategies.Bubble);
        }

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.ProfileItems, v => v.lstProfiles.ItemsSource).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.SelectedProfile, v => v.lstProfiles.SelectedItem).DisposeWith(disposables);

            this.Bind(ViewModel, vm => vm.SelectedSub, v => v.lstGroup.SelectedItem).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.ServerFilter, v => v.txtServerFilter.Text).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddSubCmd, v => v.btnAddSub).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.EditSubCmd, v => v.btnEditSub).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.EditSubCmd, v => v.menuSubEdit).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddSubCmd, v => v.menuSubAdd).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.DeleteSubCmd, v => v.menuSubDelete).DisposeWith(disposables);

            //servers delete
            this.BindCommand(ViewModel, vm => vm.EditServerCmd, v => v.menuEditServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RemoveServerCmd, v => v.menuRemoveServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RemoveDuplicateServerCmd, v => v.menuRemoveDuplicateServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.CopyServerCmd, v => v.menuCopyServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SetDefaultServerCmd, v => v.menuSetDefaultServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.ShareServerCmd, v => v.menuShareServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.GenGroupAllServerCmd, v => v.menuGenGroupAllServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.GenGroupRegionServerCmd, v => v.menuGenGroupRegionServer).DisposeWith(disposables);

            //servers move

            this.BindCommand(ViewModel, vm => vm.MoveTopCmd, v => v.menuMoveTop).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.MoveUpCmd, v => v.menuMoveUp).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.MoveDownCmd, v => v.menuMoveDown).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.MoveBottomCmd, v => v.menuMoveBottom).DisposeWith(disposables);

            //servers ping
            this.BindCommand(ViewModel, vm => vm.MixedTestServerCmd, v => v.menuMixedTestServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.TcpingServerCmd, v => v.menuTcpingServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RealPingServerCmd, v => v.menuRealPingServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.UdpTestServerCmd, v => v.menuUdpTestServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SpeedServerCmd, v => v.menuSpeedServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SortServerResultCmd, v => v.menuSortServerResult).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RemoveInvalidServerResultCmd, v => v.menuRemoveInvalidServerResult).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.FastRealPingCmd, v => v.btnFastRealPing).DisposeWith(disposables);

            //servers export
            this.BindCommand(ViewModel, vm => vm.Export2ClientConfigCmd, v => v.menuExport2ClientConfig).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.Export2ClientConfigClipboardCmd, v => v.menuExport2ClientConfigClipboard).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.Export2ShareUrlCmd, v => v.menuExport2ShareUrl).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.Export2ShareUrlBase64Cmd, v => v.menuExport2ShareUrlBase64).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.Export2InnerUriCmd, v => v.menuExport2InnerUri).DisposeWith(disposables);

            ViewModel.ShowYesNoInteraction.RegisterHandler(async interaction =>
            {
                var message = interaction.Input;
                var result = await UI.ShowYesNo(message);
                interaction.SetOutput(result == ButtonResult.Yes);
            }).DisposeWith(disposables);

            ViewModel.SaveFileDialogInteraction.RegisterHandler(async interaction =>
            {
                var viewModel = ViewModel;
                if (viewModel is null)
                {
                    interaction.SetOutput(false);
                    return;
                }
                var profileItem = interaction.Input;
                var fileName = await UI.SaveFileDialog();
                if (fileName.IsNullOrEmpty())
                {
                    interaction.SetOutput(false);
                    return;
                }
                await viewModel.Export2ClientConfigResult(fileName, profileItem);
                interaction.SetOutput(true);
            }).DisposeWith(disposables);

            ViewModel.SetClipboardDataInteraction.RegisterHandler(async interaction =>
            {
                var strData = interaction.Input;
                await AvaUtils.SetClipboardData(this, strData);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            ViewModel.ProfilesFocusInteraction.RegisterHandler(interaction =>
            {
                lstProfiles.Focus();
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            ViewModel.ShareServerInteraction.RegisterHandler(async interaction =>
            {
                var url = interaction.Input;
                if (url.IsNullOrEmpty())
                {
                    interaction.SetOutput(RxVoid.Default);
                    return;
                }
                await ShareServer(url);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            ViewModel.DispatcherRefreshServersBizInteraction.RegisterHandler(interaction =>
            {
                Dispatcher.UIThread.Post(RefreshServersBiz, DispatcherPriority.Default);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            ViewModel.AdjustMainLvColWidthInteraction.RegisterHandler(interaction =>
            {
                Dispatcher.UIThread.Post(AutofitColumnWidth, DispatcherPriority.Default);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            AppEvents.AppExitRequested
              .AsObservable()
              .ObserveOn(RxSchedulers.MainThreadScheduler)
              .Subscribe(_ => StorageUI())
              .DisposeWith(disposables);
        });

        RestoreUI();
    }

    private async void LstProfiles_Sorting(object? sender, DataGridColumnEventArgs e)
    {
        e.Handled = true;

        if (ViewModel != null && e.Column?.Tag?.ToString() != null)
        {
            await ViewModel.SortServer(e.Column.Tag.ToString());
        }

        e.Handled = false;
    }

    #region Event

    public async Task ShareServer(string url)
    {
        if (url.IsNullOrEmpty())
        {
            return;
        }

        var dialog = new QrcodeView(url);
        await DialogHost.Show(dialog);
    }

    public void RefreshServersBiz()
    {
        if (lstProfiles.SelectedIndex >= 0)
        {
            lstProfiles.ScrollIntoView(lstProfiles.SelectedItem, null);
        }
    }

    private void lstProfiles_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionSync)
        {
            return;
        }
        SyncSelectedProfiles();
    }

    private void SyncSelectedProfiles()
    {
        if (ViewModel != null)
        {
            ViewModel.SelectedProfiles = lstProfiles.SelectedItems.Cast<ProfileItemModel>().ToList();
        }
    }

    private void MenuShowTrafficColumns_Click(object? sender, RoutedEventArgs e)
    {
        var visible = !lstProfiles.Columns.Any(t => t.Tag is "TodayUp" && t.IsVisible);
        SetTrafficColumnsVisible(visible);
    }

    private bool IpInfoAllowed() => _config.SpeedTestItem.IPAPIUrl.IsNotEmpty() && !_config.UiItem.HideColumnIpInfo;

    private void SetTrafficColumnsVisible(bool visible)
    {
        foreach (var it in lstProfiles.Columns)
        {
            if (it.Tag is "TodayUp" or "TodayDown" or "TotalUp" or "TotalDown")
            {
                it.IsVisible = visible;
            }
            if (it.Tag is "IpInfo")
            {
                it.IsVisible = visible && IpInfoAllowed();
            }
        }
        menuShowTrafficColumns.IsChecked = visible;
    }

    private void LstProfiles_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var source = e.Source as Border;
        if (source?.Name == "HeaderBackground")
        {
            return;
        }

        if (_config.UiItem.DoubleClick2Activate)
        {
            ViewModel?.SetDefaultServer();
        }
        else
        {
            ViewModel?.EditServerAsync();
        }
    }

    private void LstProfiles_LoadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.Header = $" {e.Row.Index + 1}";

        // fork: the active node's row gets a faint tint (style in ForkTheme.axaml, DataGridRow.activeNode)
        e.Row.Classes.Set("activeNode", e.Row.DataContext is ProfileItemModel { IsActive: true });
    }

    private void menuSelectAll_Click(object? sender, RoutedEventArgs e)
    {
        lstProfiles.SelectAll();
    }

    private void LstProfiles_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers is KeyModifiers.Control or KeyModifiers.Meta)
        {
            switch (e.Key)
            {
                case Key.A:
                    menuSelectAll_Click(null, null);
                    break;

                case Key.C:
                    ViewModel?.Export2ShareUrlAsync(false);
                    break;

                case Key.D:
                    ViewModel?.EditServerAsync();
                    break;

                case Key.F:
                    ViewModel?.ShareServerAsync();
                    break;

                case Key.O:
                    ViewModel?.ServerSpeedtest(ESpeedActionType.Tcping);
                    break;

                case Key.R:
                    ViewModel?.ServerSpeedtest(ESpeedActionType.Realping);
                    break;

                case Key.T:
                    ViewModel?.ServerSpeedtest(ESpeedActionType.Speedtest);
                    break;

                case Key.E:
                    ViewModel?.ServerSpeedtest(ESpeedActionType.Mixedtest);
                    break;
            }
        }
        else
        {
            switch (e.Key)
            {
                case Key.Enter:
                    //case Key.Return:
                    ViewModel?.SetDefaultServer();
                    break;

                case Key.Delete:
                case Key.Back:
                    ViewModel?.RemoveServerAsync();
                    break;

                case Key.T:
                    ViewModel?.MoveServer(EMove.Top);
                    break;

                case Key.U:
                    ViewModel?.MoveServer(EMove.Up);
                    break;

                case Key.D:
                    ViewModel?.MoveServer(EMove.Down);
                    break;

                case Key.B:
                    ViewModel?.MoveServer(EMove.Bottom);
                    break;

                case Key.Escape:
                    ViewModel?.ServerSpeedtestStop();
                    break;
            }
        }
    }

    private void BtnAutofitColumnWidth_Click(object? sender, RoutedEventArgs e)
    {
        AutofitColumnWidth();
    }

    private void AutofitColumnWidth()
    {
        try
        {
            //First scroll horizontally to the initial position to avoid the control crash bug
            if (lstProfiles.SelectedIndex >= 0)
            {
                lstProfiles.ScrollIntoView(lstProfiles.SelectedItem, lstProfiles.Columns[0]);
            }
            else
            {
                var model = lstProfiles.ItemsSource.Cast<ProfileItemModel>();
                if (model.Any())
                {
                    lstProfiles.ScrollIntoView(model.First(), lstProfiles.Columns[0]);
                }
                else
                {
                    return;
                }
            }

            foreach (var it in lstProfiles.Columns)
            {
                it.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    private void TxtServerFilter_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Return)
        {
            ViewModel?.RefreshServers();
        }
    }

    #endregion Event

    #region UI

    private void RestoreUI()
    {
        try
        {
            var lvColumnItem = _config.UiItem.MainColumnItem.OrderBy(t => t.Index).ToList();
            var displayIndex = 0;
            foreach (var item in lvColumnItem)
            {
                foreach (var item2 in lstProfiles.Columns)
                {
                    if (item2.Tag == null)
                    {
                        continue;
                    }
                    if (item2.Tag.Equals(item.Name))
                    {
                        if (item.Width < 0)
                        {
                            item2.IsVisible = false;
                        }
                        else
                        {
                            item2.Width = new DataGridLength(item.Width, DataGridLengthUnitType.Pixel);
                            item2.DisplayIndex = displayIndex++;
                        }
                        if (item.Name.StartsWith("to", StringComparison.CurrentCultureIgnoreCase))
                        {
                            item2.IsVisible = _config.GuiItem.EnableStatistics && item.Width >= 0;
                        }
                        if (item.Name.Equals("IpInfo", StringComparison.CurrentCultureIgnoreCase))
                        {
                            item2.IsVisible = _config.GuiItem.EnableStatistics && item.Width >= 0 && IpInfoAllowed();
                        }
                    }
                }
            }
            menuShowTrafficColumns.IsVisible = _config.GuiItem.EnableStatistics;
            menuTrafficSeparator.IsVisible = _config.GuiItem.EnableStatistics;
            // IP info follows the traffic statistics columns toggle.
            SetTrafficColumnsVisible(lstProfiles.Columns.Any(t => t.Tag is "TodayUp" && t.IsVisible));
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    private void StorageUI()
    {
        try
        {
            List<ColumnItem> lvColumnItem = [];
            foreach (var item2 in lstProfiles.Columns)
            {
                if (item2.Tag == null)
                {
                    continue;
                }
                lvColumnItem.Add(new()
                {
                    Name = (string)item2.Tag,
                    Width = (int)(item2.IsVisible == true ? item2.ActualWidth : -1),
                    Index = item2.DisplayIndex
                });
            }
            _config.UiItem.MainColumnItem = lvColumnItem;
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    #endregion UI

    #region Drag select

    private const double DragSelectThreshold = 4;

    private bool _suppressSelectionSync;
    private bool _dragSelectPressArmed; // the press started on a row and may become a drag selection
    private bool _dragSelectActive; // the pointer moved past the threshold, the range follows the pointer
    private int _dragSelectAnchor = -1;
    private int _dragSelectFirst = -1;
    private int _dragSelectLast = -1;
    private int _dragSelectCurrent = -1; // item index the range currently ends at while the pointer is outside the rows
    private double _dragSelectStartY;
    private double _dragSelectLastY;
    private IPointer? _dragSelectPointer;
    private DispatcherTimer? _dragSelectTimer;

    // Tunnel, so it runs before the DataGrid changes the selection for this press.
    private void LstProfiles_DragSelectPressed(object? sender, PointerPressedEventArgs e)
    {
        ResetDragSelect();

        if (!e.GetCurrentPoint(lstProfiles).Properties.IsLeftButtonPressed)
        {
            return;
        }
        if ((e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Shift | KeyModifiers.Meta)) != 0)
        {
            return;
        }
        // Only presses inside the rows (not the scroll bar or headers) start a drag selection.
        if (e.Source is not Visual source || source.FindAncestorOfType<DataGridRow>(true) == null)
        {
            return;
        }
        // The anchor is the row drawn under the press, the same geometry UpdateDragSelection uses for the current row.
        // e.Source is not used for the row, because its hit-tested row can differ from the drawn row (off by one).
        var startY = e.GetPosition(lstProfiles).Y;
        if (!TryGetRowLayout(out var rows, out _, out _))
        {
            return;
        }
        var rowIndex = DragSelectionHelper.GetRowIndexAt(rows.Select(r => (r.Top, r.Height)).ToList(), startY);
        if (rowIndex < 0)
        {
            return;
        }
        var item = rows[rowIndex].Item;
        var index = ViewModel?.ProfileItems.IndexOf(item) ?? -1;
        if (index < 0)
        {
            return;
        }
        // Drag and drop sort on: a press on an already selected row keeps starting a row drag.
        if (_config.UiItem.EnableDragDropSort && lstProfiles.SelectedItems.Contains(item))
        {
            return;
        }

        _dragSelectAnchor = index;
        _dragSelectStartY = startY;
        _dragSelectPressArmed = true;
    }

    private void LstProfiles_DragSelectMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragSelectPressArmed)
        {
            return;
        }
        if (!e.GetCurrentPoint(lstProfiles).Properties.IsLeftButtonPressed)
        {
            ResetDragSelect();
            return;
        }

        _dragSelectLastY = e.GetPosition(lstProfiles).Y;
        if (!_dragSelectActive)
        {
            if (Math.Abs(_dragSelectLastY - _dragSelectStartY) <= DragSelectThreshold)
            {
                return;
            }
            _dragSelectActive = true;
            _dragSelectPointer = e.Pointer;
            e.Pointer.Capture(lstProfiles);
            _dragSelectTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(60), DispatcherPriority.Normal, (_, _) =>
            {
                if (_dragSelectActive)
                {
                    UpdateDragSelection();
                }
            });
            _dragSelectTimer.Start();
        }
        UpdateDragSelection();
    }

    private void LstProfiles_DragSelectReleased(object? sender, PointerReleasedEventArgs e)
    {
        ResetDragSelect();
    }

    private void LstProfiles_DragSelectCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _dragSelectPointer = null; // capture is already gone, so ResetDragSelect must not release it again
        ResetDragSelect();
    }

    private void ResetDragSelect()
    {
        _dragSelectTimer?.Stop();
        var pointer = _dragSelectPointer;
        _dragSelectPointer = null;
        pointer?.Capture(null);

        _dragSelectPressArmed = false;
        _dragSelectActive = false;
        _dragSelectAnchor = -1;
        _dragSelectFirst = -1;
        _dragSelectLast = -1;
        _dragSelectCurrent = -1;
    }

    // Rows currently drawn in the list, in list coordinates, with the band the rows are drawn in.
    private bool TryGetRowLayout(out List<(ProfileItemModel Item, double Top, double Height)> rows, out double bodyTop, out double bodyBottom)
    {
        rows = new List<(ProfileItemModel Item, double Top, double Height)>();
        bodyTop = 0;
        bodyBottom = 0;

        Visual? presenter = null;
        foreach (var row in lstProfiles.GetVisualDescendants().OfType<DataGridRow>())
        {
            if (!row.IsVisible || row.DataContext is not ProfileItemModel rowItem)
            {
                continue; // recycled rows stay in the presenter hidden, with stale geometry
            }
            if (row.TranslatePoint(default, lstProfiles) is not { } origin)
            {
                continue;
            }
            rows.Add((rowItem, origin.Y, row.Bounds.Height));
            presenter ??= row.GetVisualParent();
        }
        if (rows.Count == 0 || presenter == null)
        {
            return false;
        }
        if (presenter.TranslatePoint(default, lstProfiles) is not { } bodyOrigin)
        {
            return false;
        }
        bodyTop = bodyOrigin.Y;
        bodyBottom = bodyTop + presenter.Bounds.Height;
        return true;
    }

    // Finds the row under the pointer. While the pointer is above or below the rows, the range keeps growing by
    // item index (not by realized rows): the first tick outside starts at the visible edge row, every later tick
    // moves the index by the auto-scroll step, and the grid scrolls to that item so it becomes realized.
    private void UpdateDragSelection()
    {
        var items = ViewModel?.ProfileItems;
        if (items == null || items.Count == 0 || _dragSelectAnchor < 0)
        {
            return;
        }
        if (!TryGetRowLayout(out var rows, out var bodyTop, out var bodyBottom))
        {
            return;
        }

        var y = _dragSelectLastY;
        var step = DragSelectionHelper.GetAutoScrollStep(y, bodyTop, bodyBottom);
        int target;
        if (step == 0)
        {
            _dragSelectCurrent = -1;
            var rowIndex = DragSelectionHelper.GetRowIndexAt(rows.Select(r => (r.Top, r.Height)).ToList(), y);
            if (rowIndex < 0)
            {
                return;
            }
            target = items.IndexOf(rows[rowIndex].Item);
        }
        else
        {
            if (_dragSelectCurrent < 0)
            {
                var direction = Math.Sign(step);
                var candidates = direction < 0
                    ? rows.Where(r => r.Top + r.Height > bodyTop).ToList()
                    : rows.Where(r => r.Top < bodyBottom).ToList();
                if (candidates.Count == 0)
                {
                    candidates = rows;
                }
                var edge = direction < 0 ? candidates.MinBy(r => r.Top) : candidates.MaxBy(r => r.Top);
                _dragSelectCurrent = items.IndexOf(edge.Item);
                if (_dragSelectCurrent < 0)
                {
                    return;
                }
            }
            _dragSelectCurrent = DragSelectionHelper.Advance(_dragSelectCurrent, step, items.Count);
            target = _dragSelectCurrent;
        }
        if (target < 0)
        {
            return;
        }

        ApplyDragSelectRange(_dragSelectAnchor, target);
        if (step != 0)
        {
            lstProfiles.ScrollIntoView(items[target], null);
        }
    }

    // Changes only the rows that differ from the current range, and publishes the selection once at the end.
    private void ApplyDragSelectRange(int anchor, int current)
    {
        var items = ViewModel?.ProfileItems;
        if (items == null)
        {
            return;
        }
        var range = DragSelectionHelper.GetRange(anchor, current, items.Count);
        if (range == null)
        {
            return;
        }
        var first = range.Value.First;
        var last = range.Value.Last;
        if (first == _dragSelectFirst && last == _dragSelectLast)
        {
            return;
        }
        _dragSelectFirst = first;
        _dragSelectLast = last;

        var selected = lstProfiles.SelectedItems;
        _suppressSelectionSync = true;
        try
        {
            for (var i = selected.Count - 1; i >= 0; i--)
            {
                var index = selected[i] is ProfileItemModel selectedItem ? items.IndexOf(selectedItem) : -1;
                if (index < first || index > last)
                {
                    selected.RemoveAt(i);
                }
            }
            for (var i = first; i <= last; i++)
            {
                if (!selected.Contains(items[i]))
                {
                    selected.Add(items[i]);
                }
            }
        }
        finally
        {
            _suppressSelectionSync = false;
        }
        SyncSelectedProfiles();
    }

    #endregion Drag select

    #region Drag and Drop

    private static readonly DataFormat<ProfileItemModel> LstProfilesRowFormat =
        DataFormat.CreateInProcessFormat<ProfileItemModel>("LstProfilesRow");

    private (Point, PointerPressedEventArgs)? _dragStartPoint;

    private void LstProfiles_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_dragSelectPressArmed)
        {
            return; //fork: this press drag-selects rows, so it must not start a row drag
        }
        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed)
        {
            _dragStartPoint = (e.GetPosition(this), e);
        }
    }

    private async void LstProfiles_PointerMoved(object? sender, PointerEventArgs e)
    {
        try
        {
            if (_dragStartPoint == null)
            {
                return;
            }

            var properties = e.GetCurrentPoint(this).Properties;
            if (!properties.IsLeftButtonPressed)
            {
                _dragStartPoint = null;
                return;
            }

            var currentPoint = e.GetPosition(this);
            var startPoint = _dragStartPoint.Value.Item1;
            var delta = startPoint - currentPoint;

            var threshold = new Vector(4, 4);

            if (!(Math.Abs(delta.X) >= threshold.X) && !(Math.Abs(delta.Y) >= threshold.Y))
            {
                return;
            }
            var dragStartEventArgs = _dragStartPoint.Value.Item2;
            _dragStartPoint = null;

            if (e.Source is not Visual visualSource)
            {
                return;
            }
            var row = visualSource.FindAncestorOfType<DataGridRow>(true);
            if (row?.DataContext == null)
            {
                return;
            }

            e.Handled = true;

            var dragData = new DataTransfer();
            var item = DataTransferItem.Create(LstProfilesRowFormat, row.DataContext as ProfileItemModel);

            dragData.Add(item);

            await DragDrop.DoDragDropAsync(dragStartEventArgs, dragData, DragDropEffects.Move);
        }
        catch
        {
            // Ignore
        }
    }

    private void LstProfiles_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragStartPoint = null;
    }

    private void LstProfiles_DragOver(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(LstProfilesRowFormat))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }
        e.DragEffects = DragDropEffects.Move;
    }

    private void LstProfiles_Drop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(LstProfilesRowFormat))
        {
            return;
        }
        ProfileItemModel? sourceItem = null;
        foreach (var item in e.DataTransfer.Items)
        {
            if (!item.Formats.Contains(LstProfilesRowFormat))
            {
                continue;
            }
            if (item.TryGetRaw(LstProfilesRowFormat) is not ProfileItemModel model)
            {
                continue;
            }
            sourceItem = model;
            break;
        }
        if (sourceItem == null)
        {
            return;
        }
        if (e.Source is not Visual visualTarget)
        {
            return;
        }

        var targetRow = visualTarget.FindAncestorOfType<DataGridRow>(true);
        if (targetRow is not { DataContext: ProfileItemModel targetItem })
        {
            return;
        }
        if (ReferenceEquals(sourceItem, targetItem))
        {
            return;
        }
        if (lstProfiles.ItemsSource is not IList<ProfileItemModel> items)
        {
            return;
        }
        var oldIndex = items.IndexOf(sourceItem);
        var newIndex = items.IndexOf(targetItem);
        if (oldIndex >= 0 && newIndex >= 0)
        {
            ViewModel?.MoveServerTo(oldIndex, targetItem);
        }
    }

    #endregion Drag and Drop
}
