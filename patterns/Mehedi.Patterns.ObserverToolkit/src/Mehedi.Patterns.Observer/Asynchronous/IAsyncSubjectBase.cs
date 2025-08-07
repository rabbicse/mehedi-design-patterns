namespace Mehedi.Patterns.Observer.Common;


public interface IAsyncSubjectBase : IDisposable
{
    /// <summary>
    /// Unsubscribes all observers from this subject.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task UnsubscribeAsync(object sender);

    /// <summary>
    /// Unsubscribes all observers from this subject.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task UnsubscribeAsync();
}

