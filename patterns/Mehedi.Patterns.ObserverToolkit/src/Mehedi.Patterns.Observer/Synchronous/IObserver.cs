namespace Mehedi.Patterns.Observer.Synchronous;

/// <summary>
/// Represents an observer that receives notifications from a subject
/// in the Observer Pattern for synchronous communication.
/// </summary>
/// <typeparam name="T">The type of data the observer receives from the subject.</typeparam>
public interface IObserver<T>
{
    /// <summary>
    /// Called by the subject to indicate that no more updates will be sent.
    /// </summary>
    void OnCompleted();

    /// <summary>
    /// Called by the subject to notify the observer of a new update.
    /// </summary>
    /// <param name="value">The updated value sent by the subject.</param>
    void OnUpdate(T value);
}
