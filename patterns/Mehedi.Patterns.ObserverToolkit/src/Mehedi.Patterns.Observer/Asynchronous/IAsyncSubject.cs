using Mehedi.Patterns.Observer.Common;

namespace Mehedi.Patterns.Observer.Asynchronous;

/// <summary>
/// Represents an asynchronous subject that can notify observers of state changes.
/// </summary>
/// <typeparam name="T">The type of the notification value.</typeparam>
public interface IAsyncSubject<T> : IAsyncSubjectBase
{
    /// <summary>
    /// Subscribes an observer to the subject.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An IDisposable that can be used to unsubscribe.</returns>
    IDisposable Subscribe(IAsyncObserver<T> observer);

    /// <summary>
    /// Notifies all subscribed observers with the specified value.
    /// </summary>
    /// <param name="value">The value to notify observers with.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task NotifyAsync(T value);
}

