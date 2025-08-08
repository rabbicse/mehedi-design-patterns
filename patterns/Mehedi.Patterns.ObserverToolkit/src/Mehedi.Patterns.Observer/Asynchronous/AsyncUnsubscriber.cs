namespace Mehedi.Patterns.Observer.Asynchronous;

/// <summary>
/// Represents a disposable handler used to unsubscribe an asynchronous observer from a subject.
/// </summary>
/// <typeparam name="T">The type of data being observed asynchronously.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="AsyncUnsubscriber{T}"/> class.
/// </remarks>
/// <param name="observers">The list of currently subscribed asynchronous observers.</param>
/// <param name="observer">The specific asynchronous observer to unsubscribe.</param>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="observers"/> or <paramref name="observer"/> is null.</exception>
public class AsyncUnsubscriber<T>(List<IAsyncObserver<T>> observers, IAsyncObserver<T> observer) : IDisposable
{
    private readonly List<IAsyncObserver<T>> _observers = observers ?? throw new ArgumentNullException(nameof(observers));
    private readonly IAsyncObserver<T> _observer = observer ?? throw new ArgumentNullException(nameof(observer));
    private bool _disposed;

    /// <summary>
    /// Unsubscribes the observer by removing it from the observer list.
    /// This method is thread-safe and idempotent.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        lock (_observers)
        {
            _observers.Remove(_observer);
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

