using Mehedi.Patterns.Observer.Synchronous;

namespace Mehedi.Patterns.Observer.Asynchronous;

/// <summary>
/// Represents an asynchronous subject that maintains a list of observers and notifies them of state changes.
/// </summary>
/// <typeparam name="T">The type of the notification value.</typeparam>
public class AsyncSubject<T> : IAsyncSubject<T>
{
    private readonly List<IAsyncObserver<T>> _observers = new();
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Gets the current property value of the subject.
    /// </summary>
    public T? Property { get; private set; }

    /// <summary>
    /// Subscribes an observer to the subject.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An IDisposable that can be used to unsubscribe.</returns>
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
    /// Notifies all subscribed observers with the specified value.
    /// </summary>
    /// <param name="value">The value to notify observers with.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task NotifyAsync(T value)
    {
        if (_disposed) return;

        Property = value;

        IAsyncObserver<T>[] observersCopy;
        lock (_lock)
        {
            observersCopy = _observers.ToArray();
        }

        // Run all observers concurrently
        var notificationTasks = observersCopy.Select(observer =>
            SafeNotifyObserverAsync(observer, value));

        await Task.WhenAll(notificationTasks);
    }

    private async Task SafeNotifyObserverAsync(IAsyncObserver<T> observer, T value)
    {
        try
        {
            await observer.OnUpdateAsync(value).ConfigureAwait(false);
        }
        catch
        {
            // Log or handle observer errors as needed
            // Consider removing faulty observers
        }
    }

    public async Task UnsubscribeAsync(object sender)
    {
        if (_disposed) return;

        List<Task> completionTasks = new();
        List<IAsyncObserver<T>> observersToRemove = new();

        lock (_lock)
        {
            observersToRemove = _observers
                .OfType<AsyncObserver<T>>()
                .Where(o => o.Sender == sender)
                .Cast<IAsyncObserver<T>>() // Fix: Cast to interface type
                .ToList();

            foreach (var observer in observersToRemove)
            {
                completionTasks.Add(observer.OnCompletedAsync());
                _observers.Remove(observer);
            }
        }

        await Task.WhenAll(completionTasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Unsubscribes all observers from this subject.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task UnsubscribeAsync()
    {
        if (_disposed) return;

        List<Task> completionTasks;
        lock (_lock)
        {
            completionTasks = _observers.Select(observer =>
                observer.OnCompletedAsync()).ToList();
            _observers.Clear();
        }

        await Task.WhenAll(completionTasks).ConfigureAwait(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

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