namespace Mehedi.Patterns.Observer.Asynchronous;

/// <summary>
/// Represents an asynchronous observer that can subscribe to a subject and react to asynchronous notifications.
/// </summary>
/// <typeparam name="T">The type of the notification value.</typeparam>
public class AsyncObserver<T>(object sender, Func<T, Task> asyncAction) : IAsyncObserver<T>, IDisposable
{
    private IDisposable? _unsubscriber;
    private bool _disposed;

    private readonly Func<T, Task> _asyncAction = asyncAction
        ?? throw new ArgumentNullException(nameof(asyncAction));

    /// <summary>
    /// Gets the sender object associated with this observer.
    /// Useful for grouping or identifying observers when unsubscribing.
    /// </summary>
    public object Sender { get; } = sender
        ?? throw new ArgumentNullException(nameof(sender));

    /// <summary>
    /// Subscribes this observer to the specified asynchronous subject.
    /// </summary>
    /// <param name="observable">The asynchronous subject to subscribe to.</param>
    /// <exception cref="ArgumentNullException">Thrown if the observable is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the observer has been disposed.</exception>
    public void Subscribe(IAsyncSubject<T> observable)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(AsyncObserver<T>));
        _unsubscriber = observable?.Subscribe(this)
            ?? throw new ArgumentNullException(nameof(observable));
    }

    /// <summary>
    /// Called when the subject sends an update.
    /// Executes the provided asynchronous action with the notification value.
    /// </summary>
    /// <param name="value">The value provided by the subject.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnUpdateAsync(T value)
    {
        if (!_disposed)
        {
            await _asyncAction(value).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Called when the subject completes its notification sequence.
    /// Unsubscribes the observer from the subject.
    /// </summary>
    /// <returns>A task representing the asynchronous completion.</returns>
    public Task OnCompletedAsync()
    {
        Unsubscribe();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Manually unsubscribes the observer from the subject.
    /// </summary>
    public void Unsubscribe()
    {
        if (_disposed) return;

        _unsubscriber?.Dispose();
        _unsubscriber = null;
    }

    /// <summary>
    /// Disposes the observer and unsubscribes it from the subject.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        Unsubscribe();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
