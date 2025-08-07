namespace Mehedi.Patterns.Observer.Synchronous;

/// <summary>
/// Represents a generic subject that maintains a list of observers and notifies them of state changes.
/// Implements both <see cref="IAsyncSubject{T}"/> and <see cref="ISubjectBase"/> interfaces.
/// </summary>
/// <typeparam name="T">The type of the notification value.</typeparam>
public class Subject<T> : IAsyncSubject<T>, IDisposable
{
    #region Field(s)
    private readonly List<IObserver<T>> _observers = new();
    private readonly object _lock = new();
    private bool _disposed;
    #endregion

    /// <summary>
    /// Gets the current property value of the subject.
    /// </summary>
    public T? Property { get; private set; }

    /// <summary>
    /// Subscribes an observer to the subject.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An IDisposable that can be used to unsubscribe.</returns>
    /// <exception cref="ArgumentNullException">Thrown if observer is null.</exception>
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
    /// Unsubscribes all observers associated with the specified sender.
    /// </summary>
    /// <param name="sender">The sender whose observers should be unsubscribed.</param>
    public void Unsubscribe(object sender)
    {
        if (_disposed) return;

        lock (_lock)
        {
            var observersToRemove = _observers
                .OfType<Observer<T>>()
                .Where(o => o.Sender == sender)
                .ToList();

            foreach (var observer in observersToRemove)
            {
                _observers.Remove(observer);
                observer.Unsubscribe();
            }
        }
    }

    /// <summary>
    /// Unsubscribes all observers from this subject.
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

    /// <summary>
    /// Notifies all subscribed observers with the specified value.
    /// </summary>
    /// <param name="value">The value to notify observers with.</param>
    public void Notify(T value)
    {
        if (_disposed) return;

        foreach (var observer in _observers.ToArray())
        {
            try
            {
                observer.OnUpdate(value);
            }
            catch
            {
                // Log error if needed
                RemoveFaultyObserver(observer);
            }
        }
    }

    private void RemoveFaultyObserver(IObserver<T> observer)
    {
        lock (_lock)
        {
            _observers.Remove(observer);
        }
    }

    /// <summary>
    /// Disposes the subject and unsubscribes all observers.
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