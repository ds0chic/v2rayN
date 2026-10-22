using System.Collections;
using System.Collections.Specialized;

namespace v2rayN.Desktop.Common;

// Empty-state wiring shared by the list views (see the `Border.emptyState` styles in ForkTheme.axaml).
public static class CdsStates
{
    /// <summary>
    /// Keeps <paramref name="emptyState"/> visible while <paramref name="collection"/> has no items. It is shown only
    /// after the collection has stayed empty for <paramref name="graceMs"/>, so a list that is being cleared and
    /// reloaded never flashes the empty state; it hides at once when an item arrives. <paramref name="onShow"/> runs
    /// right before the element is shown (for example to pick the "no match" text). Dispose to stop watching.
    /// </summary>
    public static IDisposable BindEmpty(Control emptyState, INotifyCollectionChanged collection, Action? onShow = null, int graceMs = 300)
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(graceMs) };

        bool IsEmpty() => collection is ICollection c ? c.Count == 0 : !((IEnumerable)collection).GetEnumerator().MoveNext();

        void Show()
        {
            timer.Stop();
            if (!IsEmpty())
            {
                return;
            }

            onShow?.Invoke();
            emptyState.IsVisible = true;
        }

        void Update()
        {
            if (IsEmpty())
            {
                if (!emptyState.IsVisible && !timer.IsEnabled)
                {
                    timer.Start();
                }
                else if (emptyState.IsVisible)
                {
                    onShow?.Invoke();
                }
            }
            else
            {
                timer.Stop();
                emptyState.IsVisible = false;
            }
        }

        void OnChanged(object? sender, NotifyCollectionChangedEventArgs e) => Dispatcher.UIThread.Post(Update);

        timer.Tick += (_, _) => Show();
        emptyState.IsVisible = false;
        collection.CollectionChanged += OnChanged;
        Update();

        return new Unsubscriber(() =>
        {
            timer.Stop();
            collection.CollectionChanged -= OnChanged;
        });
    }

    private sealed class Unsubscriber(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;

        public void Dispose()
        {
            _dispose?.Invoke();
            _dispose = null;
        }
    }
}
