namespace Mehedi.Patterns.Observer.Asynchronous;

/// <summary>
/// Handles unsubscription for async observers
/// </summary>
internal class AsyncUnsubscriber<T>(List<IAsyncObserver<T>> observers, IAsyncObserver<T> observer) : IDisposable
{
    private readonly List<IAsyncObserver<T>> _observers = observers ?? throw new ArgumentNullException(nameof(observers));
    private readonly IAsyncObserver<T> _observer = observer ?? throw new ArgumentNullException(nameof(observer));
    private bool _disposed;

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

