namespace Mehedi.Patterns.Observer.Synchronous;

/// <summary>
/// Represents a subject (also known as an observable) that can be observed by one or more observers.
/// </summary>
/// <typeparam name="T">The type of data that the subject emits to its observers.</typeparam>
public interface ISubject<T> : ISubjectBase
{
    /// <summary>
    /// Subscribes an observer to receive notifications from this subject.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> that can be used to unsubscribe the observer from the subject.
    /// </returns>
    IDisposable Subscribe(IObserver<T> observer);

    /// <summary>
    /// Notifies all subscribed observers with the specified value.
    /// </summary>
    /// <param name="value">The value to send to the observers.</param>
    void Notify(T value);
}

