namespace Mehedi.Patterns.Observer.Asynchronous;

/// <summary>
/// Represents an asynchronous observer that can subscribe to a subject and react to notifications.
/// </summary>
/// <typeparam name="T">The type of the notification value.</typeparam>
public class AsyncObserver<T> : IAsyncObserver<T>, IDisposable
{
    private IDisposable? _unsubscriber;
    private bool _disposed;
    private readonly Func<T, Task> _asyncAction;

    /// <summary>
    /// Gets the sender object associated with this observer.
    /// </summary>
    public object Sender { get; }

    /// <summary>
    /// Initializes a new instance of the AsyncObserver class.
    /// </summary>
    /// <param name="sender">The sender object to associate with this observer.</param>
    /// <param name="asyncAction">The async action to execute when notified.</param>
    public AsyncObserver(object sender, Func<T, Task> asyncAction)
    {
        Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _asyncAction = asyncAction ?? throw new ArgumentNullException(nameof(asyncAction));
    }

    /// <summary>
    /// Subscribes this observer to the specified subject.
    /// </summary>
    /// <param name="observable">The subject to subscribe to.</param>
    public void Subscribe(IAsyncSubject<T> observable)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(AsyncObserver<T>));
        _unsubscriber = observable?.Subscribe(this) ?? throw new ArgumentNullException(nameof(observable));
    }

    /// <summary>
    /// Called when the subject sends a notification.
    /// </summary>
    /// <param name="value">The notification value.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task OnUpdateAsync(T value)
    {
        if (!_disposed)
        {
            await _asyncAction(value).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Called when the subject has completed sending notifications.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task OnCompletedAsync()
    {
        Unsubscribe();
        return Task.CompletedTask;
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

    public void Dispose()
    {
        if (_disposed) return;

        Unsubscribe();
        _disposed = true;
    }
}