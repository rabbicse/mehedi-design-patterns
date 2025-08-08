namespace Mehedi.Patterns.Observer.Synchronous;

/// <summary>
/// Represents a disposable handle that allows an observer to unsubscribe from a subject.
/// </summary>
/// <typeparam name="T">The type of data being observed.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="Unsubscriber{T}"/> class.
/// </remarks>
/// <param name="observers">The list of registered observers in the subject.</param>
/// <param name="observer">The specific observer that will be unsubscribed when disposed.</param>
public class Unsubscriber<T>(List<IObserver<T>> observers, IObserver<T> observer) : IDisposable
{
    private readonly List<IObserver<T>> _observers = observers ?? throw new ArgumentNullException(nameof(observers));
    private readonly IObserver<T> _observer = observer ?? throw new ArgumentNullException(nameof(observer));

    /// <summary>
    /// Unsubscribes the associated observer from the subject by removing it from the observer list.
    /// </summary>
    public void Dispose()
    {
        _observers.Remove(_observer);
        GC.SuppressFinalize(this);
    }
}

