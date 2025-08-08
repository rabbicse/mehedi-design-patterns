namespace Mehedi.Patterns.Observer.Asynchronous;

/// <summary>
/// Represents an asynchronous subject that maintains a list of observers and notifies them of state changes.
/// </summary>
/// <typeparam name="T">The type of the notification value.</typeparam>
public class AsyncSubject<T> : IAsyncSubject<T>, IDisposable
{
    private readonly List<IAsyncObserver<T>> _observers = new();
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Gets the current value held by the subject.
    /// </summary>
    public T? Property { get; private set; }

    /// <summary>
    /// Subscribes an asynchronous observer to the subject.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the observer is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the subject has already been disposed.</exception>
    public IDisposable Subscribe(IAsyncObserver<T> observer)
    {
        if (observer == null) throw new ArgumentNullException(nameof(observer));
        if (_disposed) throw new ObjectDisposedException(nameof(AsyncSubject<T>));

        lock (_lock)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
            return new AsyncUnsubscriber<T>(_observers, observer);
        }
    }

    /// <summary>
    /// Notifies all subscribed observers asynchronously with the specified value.
    /// </summary>
    /// <param name="value">The value to notify observers with.</param>
    /// <returns>A task representing the asynchronous notification operation.</returns>
    public async Task NotifyAsync(T value)
    {
        if (_disposed) return;

        Property = value;

        IAsyncObserver<T>[] observersCopy;
        lock (_lock)
        {
            observersCopy = _observers.ToArray();
        }

        var notificationTasks = observersCopy
            .Select(observer => SafeNotifyObserverAsync(observer, value));

        await Task.WhenAll(notificationTasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Safely notifies an individual observer and suppresses any thrown exceptions.
    /// </summary>
    /// <param name="observer">The observer to notify.</param>
    /// <param name="value">The value to send to the observer.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SafeNotifyObserverAsync(IAsyncObserver<T> observer, T value)
    {
        try
        {
            await observer.OnUpdateAsync(value).ConfigureAwait(false);
        }
        catch
        {
            // Suppress exceptions to avoid breaking the notification loop
            // Optional: log or remove faulty observers
        }
    }

    /// <summary>
    /// Unsubscribes all observers associated with the specified sender.
    /// </summary>
    /// <param name="sender">The sender whose observers should be unsubscribed.</param>
    /// <returns>A task representing the asynchronous unsubscription operation.</returns>
    public async Task UnsubscribeAsync(object sender)
    {
        if (_disposed) return;

        List<Task> completionTasks;
        List<IAsyncObserver<T>> observersToRemove;

        lock (_lock)
        {
            observersToRemove = _observers
                .OfType<AsyncObserver<T>>()
                .Where(o => o.Sender == sender)
                .Cast<IAsyncObserver<T>>()
                .ToList();

            foreach (var observer in observersToRemove)
            {
                _observers.Remove(observer);
            }
        }

        completionTasks = observersToRemove
            .Select(o => o.OnCompletedAsync())
            .ToList();

        await Task.WhenAll(completionTasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Unsubscribes all observers from this subject and notifies them of completion.
    /// </summary>
    /// <returns>A task representing the asynchronous unsubscription operation.</returns>
    public async Task UnsubscribeAsync()
    {
        if (_disposed) return;

        List<IAsyncObserver<T>> observersCopy;
        lock (_lock)
        {
            observersCopy = _observers.ToList();
            _observers.Clear();
        }

        var completionTasks = observersCopy
            .Select(observer => observer.OnCompletedAsync())
            .ToList();

        await Task.WhenAll(completionTasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes the subject and unsubscribes all observers asynchronously.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the subject and releases resources.
    /// </summary>
    /// <param name="disposing">Indicates whether managed resources should be disposed.</param>
    protected virtual async void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            await UnsubscribeAsync().ConfigureAwait(false);
        }

        _disposed = true;
    }
}
