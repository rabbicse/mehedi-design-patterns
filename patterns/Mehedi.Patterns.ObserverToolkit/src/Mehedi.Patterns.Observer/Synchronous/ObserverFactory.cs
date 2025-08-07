using System.Collections.Concurrent;

namespace Mehedi.Patterns.Observer.Synchronous;

/// <summary>
/// Provides a thread-safe factory for creating and managing observers and subjects.
/// Implements the singleton pattern and IDisposable.
/// </summary>
public sealed class ObserverFactory : IDisposable
{
    #region Singleton Implementation
    private static readonly Lazy<ObserverFactory> _instance = new(() => new ObserverFactory());
    private bool _disposed;

    /// <summary>
    /// Gets the singleton instance of the ObserverFactory.
    /// </summary>
    public static ObserverFactory Instance => _instance.Value;

    private ObserverFactory()
    {
        _observables = new ConcurrentDictionary<string, object>();
    }
    #endregion

    #region Fields
    private readonly ConcurrentDictionary<string, object> _observables;
    #endregion

    #region Public Methods

    /// <summary>
    /// Registers a handler to the specified subject.
    /// </summary>
    /// <typeparam name="T">The type of the notification value.</typeparam>
    /// <param name="key">The unique key identifying the subject.</param>
    /// <param name="sender">The sender object to associate with the observer.</param>
    /// <param name="action">The action to execute when notified.</param>
    /// <exception cref="ArgumentNullException">Thrown if key, sender or action is null.</exception>
    public void RegisterHandler<T>(string key, object sender, Action<T> action)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ObserverFactory));
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
        if (sender == null) throw new ArgumentNullException(nameof(sender));
        if (action == null) throw new ArgumentNullException(nameof(action));

        var subject = GetOrCreateSubject<T>(key);
        var observer = new Observer<T>(sender, action);
        observer.Subscribe(subject);
    }

    /// <summary>
    /// Unregisters all handlers associated with the specified sender from the subject.
    /// </summary>
    /// <param name="key">The unique key identifying the subject.</param>
    /// <param name="sender">The sender whose handlers should be unregistered.</param>
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
    /// Unregisters all handlers from the specified subject and optionally removes it from the factory.
    /// </summary>
    /// <param name="key">The unique key identifying the subject.</param>
    /// <param name="tryRemove">Whether to remove the subject from the factory.</param>
    public void UnregisterHandler(string key, bool tryRemove = true)
    {
        if (_disposed) return;
        if (string.IsNullOrWhiteSpace(key)) return;

        if (_observables.TryGetValue(key, out var obj) && obj is ISubjectBase subject)
        {
            subject.Unsubscribe();
            subject.Dispose();

            if (tryRemove)
            {
                _observables.TryRemove(key, out _);
            }
        }
    }

    /// <summary>
    /// Unregisters all subscriptions and clears all subjects from the factory.
    /// </summary>
    public void UnregisterAllSubscription()
    {
        if (_disposed) return;

        foreach (var key in _observables.Keys.ToArray()) // Create a copy to avoid modification during iteration
        {
            UnregisterHandler(key, tryRemove: false);
        }
        _observables.Clear();
    }

    /// <summary>
    /// Notifies all observers subscribed to the specified subject.
    /// </summary>
    /// <typeparam name="T">The type of the notification value.</typeparam>
    /// <param name="key">The unique key identifying the subject.</param>
    /// <param name="value">The value to notify observers with.</param>
    public void Notify<T>(string key, T value)
    {
        if (_disposed) return;
        if (string.IsNullOrWhiteSpace(key)) return;

        if (_observables.TryGetValue(key, out var obj) && obj is Subject<T> subject)
        {
            subject.Notify(value);
        }
    }

    /// <summary>
    /// Gets the current value of the specified subject.
    /// </summary>
    /// <typeparam name="T">The type of the subject's value.</typeparam>
    /// <param name="key">The unique key identifying the subject.</param>
    /// <returns>The current value of the subject, or default(T) if not found.</returns>
    public T? GetSubject<T>(string key)
    {
        if (_disposed) return default;
        if (string.IsNullOrWhiteSpace(key)) return default;

        return _observables.TryGetValue(key, out var obj) && obj is Subject<T> subject
            ? subject.Property
            : default;
    }

    /// <summary>
    /// Shuts down the ObserverFactory, cleaning up all resources.
    /// </summary>
    public static void Shutdown()
    {
        if (_instance.IsValueCreated)
        {
            _instance.Value.Dispose();
        }
    }

    #endregion

    #region Private Helpers

    private Subject<T> GetOrCreateSubject<T>(string key)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ObserverFactory));

        return _observables.GetOrAdd(key, k =>
        {
            var subject = new Subject<T>();
            return subject;
        }) as Subject<T> ?? throw new InvalidOperationException($"Subject type mismatch for key '{key}'.");
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Disposes the ObserverFactory and all its subjects.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        UnregisterAllSubscription();
        _disposed = true;
    }

    #endregion
}