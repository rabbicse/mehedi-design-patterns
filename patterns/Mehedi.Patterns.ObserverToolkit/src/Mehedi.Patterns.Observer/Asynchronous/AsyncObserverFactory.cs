using System.Collections.Concurrent;

namespace Mehedi.Patterns.Observer.Asynchronous;

/// <summary>
/// Provides a thread-safe singleton factory for creating and managing asynchronous observers and subjects.
/// </summary>
public sealed class AsyncObserverFactory : IDisposable
{
    private static readonly Lazy<AsyncObserverFactory> _instance = new(() => new AsyncObserverFactory());
    private readonly ConcurrentDictionary<string, object> _observables;
    private bool _disposed;

    /// <summary>
    /// Gets the singleton instance of the <see cref="AsyncObserverFactory"/>.
    /// </summary>
    public static AsyncObserverFactory Instance => _instance.Value;

    private AsyncObserverFactory()
    {
        _observables = new ConcurrentDictionary<string, object>();
    }

    /// <summary>
    /// Registers an asynchronous handler with the specified subject identified by the key.
    /// Creates the subject if it does not already exist.
    /// </summary>
    /// <typeparam name="T">The type of the notification value.</typeparam>
    /// <param name="key">The unique key identifying the subject.</param>
    /// <param name="sender">The sender object associated with the observer.</param>
    /// <param name="asyncAction">The asynchronous action to invoke when the observer is notified.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the factory has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if any argument is null or invalid.</exception>
    public void RegisterHandler<T>(string key, object sender, Func<T, Task> asyncAction)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(AsyncObserverFactory));
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
        if (sender == null) throw new ArgumentNullException(nameof(sender));
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

        var subject = GetOrCreateSubject<T>(key);
        var observer = new AsyncObserver<T>(sender, asyncAction);
        observer.Subscribe(subject);
    }

    /// <summary>
    /// Asynchronously notifies all observers subscribed to the subject identified by the key.
    /// </summary>
    /// <typeparam name="T">The type of the notification value.</typeparam>
    /// <param name="key">The unique key identifying the subject.</param>
    /// <param name="value">The notification value to send to observers.</param>
    public async Task NotifyAsync<T>(string key, T value)
    {
        if (_disposed) return;
        if (string.IsNullOrWhiteSpace(key)) return;

        if (_observables.TryGetValue(key, out var obj) && obj is IAsyncSubject<T> subject)
        {
            await subject.NotifyAsync(value).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously unregisters all subscriptions and clears all subjects from the factory.
    /// </summary>
    public async Task UnregisterAllSubscriptionAsync()
    {
        if (_disposed) return;

        var unsubscribeTasks = _observables.Keys
            .Select(key => UnregisterHandlerAsync(key, tryRemove: false))
            .ToList();

        await Task.WhenAll(unsubscribeTasks).ConfigureAwait(false);
        _observables.Clear();
    }

    /// <summary>
    /// Retrieves an existing subject identified by the key or creates a new one if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the notification value.</typeparam>
    /// <param name="key">The unique key identifying the subject.</param>
    /// <returns>The existing or newly created subject.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the factory has been disposed.</exception>
    private AsyncSubject<T> GetOrCreateSubject<T>(string key)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(AsyncObserverFactory));

        return (AsyncSubject<T>)_observables.GetOrAdd(key, _ => new AsyncSubject<T>());
    }

    /// <summary>
    /// Asynchronously unregisters all observers associated with the specified sender from the subject identified by the key.
    /// </summary>
    /// <param name="key">The unique key identifying the subject.</param>
    /// <param name="sender">The sender whose observers should be unsubscribed.</param>
    public async Task UnregisterHandlerAsync(string key, object sender)
    {
        if (_disposed) return;
        if (string.IsNullOrWhiteSpace(key)) return;
        if (sender == null) return;

        if (_observables.TryGetValue(key, out var obj) && obj is IAsyncSubjectBase subject)
        {
            await subject.UnsubscribeAsync(sender).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously unregisters all observers from the subject identified by the key and optionally removes the subject from the factory.
    /// </summary>
    /// <param name="key">The unique key identifying the subject.</param>
    /// <param name="tryRemove">Indicates whether to remove the subject from the factory after unsubscription. Defaults to true.</param>
    public async Task UnregisterHandlerAsync(string key, bool tryRemove = true)
    {
        if (_disposed) return;
        if (string.IsNullOrWhiteSpace(key)) return;

        if (_observables.TryGetValue(key, out var obj) && obj is IAsyncSubjectBase subject)
        {
            await subject.UnsubscribeAsync().ConfigureAwait(false);
            subject.Dispose();

            if (tryRemove)
            {
                _observables.TryRemove(key, out _);
            }
        }
    }

    /// <summary>
    /// Disposes the factory and unregisters all subscriptions synchronously.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        // Blocking call to async method acceptable here during disposal
        UnregisterAllSubscriptionAsync().GetAwaiter().GetResult();
        _disposed = true;
    }

    /// <summary>
    /// Shuts down the factory asynchronously, unregistering all subscriptions and disposing the factory instance.
    /// </summary>
    public static async Task ShutdownAsync()
    {
        if (_instance.IsValueCreated)
        {
            await _instance.Value.UnregisterAllSubscriptionAsync().ConfigureAwait(false);
            _instance.Value.Dispose();
        }
    }
}
