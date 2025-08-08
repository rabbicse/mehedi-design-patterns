namespace Mehedi.Patterns.Observer.Synchronous;


/// <summary>
/// Represents the base contract for a subject in the observer pattern,
/// providing core unsubscribe and disposal mechanisms.
/// </summary>
public interface ISubjectBase : IDisposable
{
    /// <summary>
    /// Unsubscribes a specific observer from the subject.
    /// </summary>
    /// <param name="sender">
    /// The observer to unsubscribe. Typically passed as the observer instance itself.
    /// </param>
    void Unsubscribe(object sender);

    /// <summary>
    /// Unsubscribes all observers from the subject.
    /// </summary>
    void Unsubscribe();
}

