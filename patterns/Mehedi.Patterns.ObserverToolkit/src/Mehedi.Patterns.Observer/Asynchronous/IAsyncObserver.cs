namespace Mehedi.Patterns.Observer.Asynchronous;

/// <summary>
/// Represents an asynchronous observer that can receive and process notifications.
/// </summary>
/// <typeparam name="T">The type of notification value.</typeparam>
public interface IAsyncObserver<T>
{
    /// <summary>
    /// Called when the subject has completed sending notifications.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task OnCompletedAsync();

    /// <summary>
    /// Called when the subject sends a notification.
    /// </summary>
    /// <param name="value">The notification value.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task OnUpdateAsync(T value);
}
