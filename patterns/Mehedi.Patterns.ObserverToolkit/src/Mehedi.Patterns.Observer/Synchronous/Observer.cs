namespace Mehedi.Patterns.Observer.Synchronous;

/// <summary>
/// Represents an observer that can subscribe to a subject and react to notifications.
/// Implements both <see cref="IObserver{T}"/> and <see cref="IDisposable"/> interfaces.
/// </summary>
/// <typeparam name="T">The type of the notification value.</typeparam>
public class Observer<T> : IObserver<T>, IDisposable
{
    private IDisposable? _unsubscriber;
    private bool _disposed;

    /// <summary>
    /// Gets the sender object associated with this observer.
    /// </summary>
    public object Sender { get; }

    private readonly Action<T> _action;

    /// <summary>
    /// Initializes a new instance of the Observer class.
    /// </summary>
    /// <param name="sender">The sender object to associate with this observer.</param>
    /// <param name="action">The action to execute when notified.</param>
    public Observer(object sender, Action<T> action)
    {
        Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Subscribes this observer to the specified subject.
    /// </summary>
    /// <param name="observable">The subject to subscribe to.</param>
    /// <exception cref="ArgumentNullException">Thrown if observable is null.</exception>
    public void Subscribe(IAsyncSubject<T> observable)
    {
        if (_disposed) ObjectDisposedException.ThrowIf(_disposed, nameof(Observer<T>));
        _unsubscriber = observable?.Subscribe(this) ?? throw new ArgumentNullException(nameof(observable));
    }

    /// <summary>
    /// Unsubscribes this observer from its subject.
    /// </summary>
    public void Unsubscribe()
    {
        if (_disposed) return;

        _unsubscriber?.Dispose();
        _unsubscriber = null;
    }

    /// <summary>
    /// Called when the subject has completed sending notifications.
    /// </summary>
    public void OnCompleted()
    {
        Unsubscribe();
    }

    /// <summary>
    /// Called when the subject sends a notification.
    /// </summary>
    /// <param name="value">The notification value.</param>
    public void OnUpdate(T value)
    {
        if (!_disposed)
        {
            _action?.Invoke(value);
        }
    }

    /// <summary>
    /// Disposes the observer and unsubscribes from its subject.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            Unsubscribe();
        }

        _disposed = true;
    }
}