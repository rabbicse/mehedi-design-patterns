namespace Mehedi.Patterns.Observer.Synchronous;

/// <summary>
/// Represents an observer that can subscribe to a subject and react to notifications.
/// Implements both <see cref="IObserver{T}"/> and <see cref="IDisposable"/> interfaces.
/// </summary>
/// <typeparam name="T">The type of the notification value.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="Observer{T}"/> class.
/// </remarks>
/// <param name="sender">An object representing the source or owner of this observer instance.</param>
/// <param name="action">The action to execute when a notification is received.</param>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="sender"/> or <paramref name="action"/> is null.</exception>
public class Observer<T>(object sender, Action<T> action) : IObserver<T>, IDisposable
{
    private IDisposable? _unsubscriber;
    private bool _disposed;

    private readonly Action<T> _action = action ?? throw new ArgumentNullException(nameof(action));

    /// <summary>
    /// Gets the sender object associated with this observer.
    /// Used for identification when unsubscribing.
    /// </summary>
    public object Sender { get; } = sender ?? throw new ArgumentNullException(nameof(sender));

    /// <summary>
    /// Subscribes this observer to the given subject.
    /// </summary>
    /// <param name="observable">The subject to observe.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="observable"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the observer has already been disposed.</exception>
    public void Subscribe(ISubject<T> observable)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Observer<T>));

        _unsubscriber = observable?.Subscribe(this) ?? throw new ArgumentNullException(nameof(observable));
    }

    /// <summary>
    /// Unsubscribes this observer from the subject it is observing.
    /// </summary>
    public void Unsubscribe()
    {
        if (_disposed) return;

        _unsubscriber?.Dispose();
        _unsubscriber = null;
    }

    /// <summary>
    /// Called by the subject to indicate that no further updates will be sent.
    /// Automatically unsubscribes the observer.
    /// </summary>
    public void OnCompleted()
    {
        Unsubscribe();
    }

    /// <summary>
    /// Called by the subject to notify the observer of a new update.
    /// </summary>
    /// <param name="value">The updated value.</param>
    public void OnUpdate(T value)
    {
        if (!_disposed)
        {
            _action.Invoke(value);
        }
    }

    /// <summary>
    /// Disposes the observer and unsubscribes it from the subject.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of the dispose pattern.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose or finalizer.</param>
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