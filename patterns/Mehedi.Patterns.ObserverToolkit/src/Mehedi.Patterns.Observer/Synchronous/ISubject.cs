namespace Mehedi.Patterns.Observer.Synchronous;

public interface IAsyncSubject<T> : ISubjectBase
{
    IDisposable Subscribe(IObserver<T> observer);
    void Notify(T value);
}

