using Mehedi.Patterns.Observer.Synchronous;
using System.Collections.Concurrent;

namespace Mehedi.Patterns.Observer.Asynchronous;

/// <summary>
/// Provides a thread-safe factory for creating and managing async observers and subjects.
/// </summary>
public sealed class AsyncObserverFactory : IDisposable
{
    private static readonly Lazy<AsyncObserverFactory> _instance = new(() => new AsyncObserverFactory());
    private readonly ConcurrentDictionary<string, object> _observables;
    private bool _disposed;

    /// <summary>
    /// Gets the singleton instance of the AsyncObserverFactory.
    /// </summary>
    public static AsyncObserverFactory Instance => _instance.Value;

    private AsyncObserverFactory()
    {
        _observables = new ConcurrentDictionary<string, object>();
    }

    /// <summary>
    /// Registers an async handler to the specified subject.
    /// </summary>
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
    /// Notifies all observers subscribed to the specified subject.
    /// </summary>
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
    /// Unregisters all handlers associated with the specified sender from the subject.
    /// </summary>
    public void UnregisterHandler(string key, object sender)
    {
        if (_disposed) return;
        if (string.IsNullOrWhiteSpace(key)) return;
        if (sender == null) return;

        if (_observables.TryGetValue(key, out var obj) && obj is ISubjectBase subject)
        {
            subject.Unsubscribe(sender);
        }
    }

    /// <summary>
    /// Unregisters all subscriptions and clears all subjects from the factory.
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

    private AsyncSubject<T> GetOrCreateSubject<T>(string key)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(AsyncObserverFactory));

        return (AsyncSubject<T>)_observables.GetOrAdd(key, k => new AsyncSubject<T>());
    }

    private async Task UnregisterHandlerAsync(string key, bool tryRemove = true)
    {
        if (_observables.TryGetValue(key, out var obj) && obj is IDisposable subject)
        {
            try
            {
                // Dynamically invoke UnsubscribeAsync if available
                var unsubscribeMethod = subject.GetType().GetMethod("UnsubscribeAsync");
                if (unsubscribeMethod != null)
                {
                    var result = unsubscribeMethod.Invoke(subject, null);
                    if (result is Task task)
                    {
                        await task.ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                subject.Dispose();

                if (tryRemove)
                {
                    _observables.TryRemove(key, out _);
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        // Note: We're calling the async method synchronously here
        // which is generally not recommended, but acceptable in disposal
        UnregisterAllSubscriptionAsync().GetAwaiter().GetResult();
        _disposed = true;
    }

    /// <summary>
    /// Shuts down the AsyncObserverFactory, cleaning up all resources.
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