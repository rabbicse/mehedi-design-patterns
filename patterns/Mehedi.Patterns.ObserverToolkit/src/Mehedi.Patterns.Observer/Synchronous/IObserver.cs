namespace Mehedi.Patterns.Observer.Synchronous;

public interface IObserver<T>
{
    void OnCompleted();
    void OnUpdate(T value);
}
