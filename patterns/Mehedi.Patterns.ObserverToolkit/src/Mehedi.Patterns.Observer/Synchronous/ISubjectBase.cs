namespace Mehedi.Patterns.Observer.Synchronous;


public interface ISubjectBase : IDisposable
{
    void Unsubscribe(object sender);
    void Unsubscribe();
}

