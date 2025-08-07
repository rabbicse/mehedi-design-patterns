using Mehedi.Patterns.Observer.Asynchronous;

namespace ObserverTests;


public class AsyncSubjectTests
{
    [Fact]
    public async Task Subject_ShouldNotifyAllObservers()
    {
        // Arrange
        var observer1Called = false;
        var observer2Called = false;

        var subject = new AsyncSubject<string>();
        var observer1 = new AsyncObserver<string>(new object(), async _ =>
        {
            observer1Called = true;
            await Task.Delay(10);
        });

        var observer2 = new AsyncObserver<string>(new object(), async _ =>
        {
            observer2Called = true;
            await Task.Delay(10);
        });

        // Act
        observer1.Subscribe(subject);
        observer2.Subscribe(subject);
        await subject.NotifyAsync("test");

        // Assert
        Assert.True(observer1Called);
        Assert.True(observer2Called);
    }

    [Fact]
    public async Task Subject_ShouldHandleObserverExceptions()
    {
        // Arrange
        var goodObserverCalled = false;
        var subject = new AsyncSubject<string>();

        // Bad observer that throws
        var badObserver = new AsyncObserver<string>(new object(), async _ =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Test exception");
        });

        // Good observer
        var goodObserver = new AsyncObserver<string>(new object(), async _ =>
        {
            goodObserverCalled = true;
            await Task.Delay(10);
        });

        // Act
        badObserver.Subscribe(subject);
        goodObserver.Subscribe(subject);

        var exception = await Record.ExceptionAsync(() => subject.NotifyAsync("test"));

        // Assert
        Assert.Null(exception); // Subject should swallow observer exceptions
        Assert.True(goodObserverCalled);
    }

    [Fact]
    public async Task Subject_ShouldNotNotify_AfterDisposal()
    {
        // Arrange
        var invoked = false;
        var subject = new AsyncSubject<string>();
        var observer = new AsyncObserver<string>(new object(), async _ =>
        {
            invoked = true;
            await Task.Delay(10);
        });

        observer.Subscribe(subject);

        // Act
        subject.Dispose();
        await subject.NotifyAsync("test");

        // Assert
        Assert.False(invoked);
    }
}


