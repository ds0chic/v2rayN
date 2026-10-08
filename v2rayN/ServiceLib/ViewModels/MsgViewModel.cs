namespace ServiceLib.ViewModels;

public partial class MsgViewModel : MyReactiveObject
{
    public Interaction<string, RxVoid> ShowMsgInteraction { get; } = new();

    private readonly ConcurrentQueue<string> _queueMsg = new();
    private volatile bool _lastMsgFilterNotAvailable;
    private Regex? _msgFilterRegex;
    public int NumMaxMsg => 500;

    [Reactive]
    public partial string MsgFilter { get; set; }

    [Reactive]
    public partial bool AutoRefresh { get; set; }

    public MsgViewModel()
    {
        _config = AppManager.Instance.Config;
        MsgFilter = _config.MsgUIItem.MainMsgFilter ?? string.Empty;
        AutoRefresh = _config.MsgUIItem.AutoRefresh ?? true;

        this.WhenAnyValue(
                x => x.MsgFilter)
            .Subscribe(c => DoMsgFilter());

        this.WhenAnyValue(x => x.AutoRefresh)
            .Subscribe(_ => _config.MsgUIItem.AutoRefresh = AutoRefresh);

        AppEvents.SendMsgViewRequested
            .AsObservable()
            .Subscribe(EnqueueQueueMsg);

        this.WhenActivated(disposables =>
        {
            Signal.Every(TimeSpan.FromSeconds(1))
                .Where(_ => AutoRefresh && AppManager.Instance.ShowInTaskbar)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(_ => FlushQueueToView())
                .DisposeWith(disposables);
        });
    }

    private void FlushQueueToView()
    {
        if (!AutoRefresh || _queueMsg.IsEmpty)
        {
            return;
        }

        if (!AppManager.Instance.ShowInTaskbar)
        {
            return;
        }

        var sb = new StringBuilder();
        while (_queueMsg.TryDequeue(out var msg))
        {
            sb.Append(msg);
        }

        if (sb.Length > 0)
        {
            ShowMsgInteraction.HandleSafe(sb.ToString()).Subscribe();
        }
    }

    private void EnqueueQueueMsg(string msg)
    {
        if (string.IsNullOrEmpty(msg))
        {
            return;
        }

        //filter msg
        if (_msgFilterRegex is not null && !_lastMsgFilterNotAvailable)
        {
            try
            {
                if (!_msgFilterRegex.IsMatch(msg))
                {
                    return;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                // keep the line, as Utils.IsRegexMatch does on timeout; no log entry per line
            }
        }

        var formattedMsg = msg.EndsWith(Environment.NewLine)
            ? msg
            : msg + Environment.NewLine;

        EnqueueWithLimit(formattedMsg);
    }

    private void EnqueueWithLimit(string item)
    {
        _queueMsg.Enqueue(item);

        while (_queueMsg.Count > NumMaxMsg)
        {
            _queueMsg.TryDequeue(out _);
        }
    }

    private void DoMsgFilter()
    {
        _config.MsgUIItem.MainMsgFilter = MsgFilter;
        _lastMsgFilterNotAvailable = false;
        _msgFilterRegex = null;
        if (MsgFilter.IsNotEmpty() && !TryCompileMsgFilter(MsgFilter, out _msgFilterRegex, out var error))
        {
            // One notice per filter change; an invalid filter is treated as disabled, so every line is shown.
            _lastMsgFilterNotAvailable = true;
            EnqueueWithLimit(error);
        }
    }

    // Compiles the log filter once. An invalid pattern returns false with the reason and never throws.
    public static bool TryCompileMsgFilter(string filter, out Regex? regex, out string error)
    {
        try
        {
            regex = new Regex(filter, RegexOptions.None, TimeSpan.FromSeconds(2));
            error = string.Empty;
            return true;
        }
        catch (ArgumentException ex)
        {
            regex = null;
            error = ex.Message;
            return false;
        }
    }
}
