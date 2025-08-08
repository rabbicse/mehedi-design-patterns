namespace Mehedi.Patterns.Observer.Synchronous;

/// <summary>
/// Represents a synchronous subject that maintains a list of observers and notifies them of state changes.
/// </summary>
/// <typeparam name="T">The type of the notification value.</typeparam>
public class Subject<T> : ISubject<T>, IDisposable
{
    #region Fields

    private readonly List<IObserver<T>> _observers = new();
    private readonly object _lock = new();
    private bool _disposed;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the current property value of the subject.
    /// This property is updated with the last value passed to <see cref="Notify"/>.
    /// </summary>
    public T? Property { get; private set; }

    #endregion

    #region Subscription Methods

    /// <summary>
    /// Subscribes an observer to the subject.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> instance that can be used to unsubscribe the observer.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the observer is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the subject has been disposed.</exception>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        ObjectDisposedException.ThrowIf(_disposed, nameof(Subject<T>));

        lock (_lock)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }

            return new Unsubscriber<T>(_observers, observer);
        }
    }

    /// <summary>
    /// Unsubscribes all observers associated with a specific sender object.
    /// Only works if the observer is of type <see cref="Observer{T}"/>.
    /// </summary>
    /// <param name="sender">The sender associated with observers to be removed.</param>
    public void Unsubscribe(object sender)
    {
        if (_disposed) return;

        lock (_lock)
        {
            var toRemove = _observers
                .OfType<Observer<T>>()
                .Where(o => o.Sender == sender)
                .ToList();

            foreach (var observer in toRemove)
            {
                _observers.Remove(observer);
                observer.Unsubscribe();
            }
        }
    }

    /// <summary>
    /// Unsubscribes all currently subscribed observers from the subject.
    /// </summary>
    public void Unsubscribe()
    {
        if (_disposed) return;

        lock (_lock)
        {
            foreach (var observer in _observers.ToList())
            {
                observer.OnCompleted();
            }

            _observers.Clear();
        }
    }

    #endregion

    #region Notification

    /// <summary>
    /// Notifies all subscribed observers with the specified value.
    /// </summary>
    /// <param name="value">The value to notify observers with.</param>
    /// <remarks>
    /// Updates the <see cref="Property"/> to reflect the latest notification value.
    /// Observers that throw exceptions will be removed.
    /// </remarks>
    public void Notify(T value)
    {
        if (_disposed) return;

        Property = value;

        foreach (var observer in _observers.ToArray())
        {
            try
            {
                observer.OnUpdate(value);
            }
            catch
            {
                RemoveFaultyObserver(observer);
                // Optional: log or report observer failure
            }
        }
    }

    /// <summary>
    /// Removes an observer from the list if it throws an exception during notification.
    /// </summary>
    /// <param name="observer">The faulty observer to remove.</param>
    private void RemoveFaultyObserver(IObserver<T> observer)
    {
        lock (_lock)
        {
            _observers.Remove(observer);
        }
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Releases all resources used by the subject and unsubscribes all observers.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the subject and its resources.
    /// </summary>
    /// <param name="disposing">
    /// true to release both managed and unmanaged resources; false to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            Unsubscribe();
        }

        _disposed = true;
    }

    #endregion
}
