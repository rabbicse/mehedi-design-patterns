namespace Mehedi.Patterns.Observer.Asynchronous;

/// <summary>
/// Handles unsubscription for async observers
/// </summary>
internal class AsyncUnsubscriber<T> : IDisposable
{
    private readonly List<IAsyncObserver<T>> _observers;
    private readonly IAsyncObserver<T> _observer;
    private bool _disposed;

    public AsyncUnsubscriber(List<IAsyncObserver<T>> observers, IAsyncObserver<T> observer)
    {
        _observers = observers ?? throw new ArgumentNullException(nameof(observers));
        _observer = observer ?? throw new ArgumentNullException(nameof(observer));
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_observers)
        {
            if (_observer != null && _observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
        }
        _disposed = true;
    }
}

