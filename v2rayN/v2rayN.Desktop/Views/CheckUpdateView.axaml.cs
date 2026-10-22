namespace v2rayN.Desktop.Views;

public partial class CheckUpdateView : ReactiveUserControl<CheckUpdateViewModel>
{
    public CheckUpdateView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.CheckUpdateModels, v => v.lstCheckUpdates.ItemsSource).DisposeWith(disposables);

            this.Bind(ViewModel, vm => vm.EnableUpdateViaProxy, v => v.togEnableUpdateViaProxy.IsChecked).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.CheckOnlyCmd, v => v.btnCheckOnly).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.CheckUpdateCmd, v => v.btnCheckUpdate).DisposeWith(disposables);

            if (ViewModel is { } vm)
            {
                bool checkingOnly = false, checkingUpdate = false;
                void UpdateChecking() => barChecking.IsVisible = checkingOnly || checkingUpdate;
                vm.CheckOnlyCmd.IsExecuting
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(running => { checkingOnly = running; UpdateChecking(); })
                    .DisposeWith(disposables);
                vm.CheckUpdateCmd.IsExecuting
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(running => { checkingUpdate = running; UpdateChecking(); })
                    .DisposeWith(disposables);
            }
        });
    }
}
