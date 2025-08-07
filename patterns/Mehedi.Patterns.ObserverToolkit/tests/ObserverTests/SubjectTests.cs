using Mehedi.Patterns.Observer.Synchronous;

namespace ObserverTests;

public class SubjectTests
{
    [Fact]
    public void Subject_ShouldNotifyAllObservers()
    {
        // Arrange
        var observer1Called = false;
        var observer2Called = false;

        var subject = new Subject<string>();
        var observer1 = new Observer<string>(new object(), async _ =>
        {
            observer1Called = true;
            await Task.Delay(10);
        });

        var observer2 = new Observer<string>(new object(), async _ =>
        {
            observer2Called = true;
            await Task.Delay(10);
        });

        // Act
        observer1.Subscribe(subject);
        observer2.Subscribe(subject);
        subject.Notify("test");

        // Assert
        Assert.True(observer1Called);
        Assert.True(observer2Called);
    }

    [Fact]
    public void Subject_ShouldHandleObserverExceptions()
    {
        // Arrange
        var goodObserverCalled = false;
        var subject = new Subject<string>();

        // Bad observer that throws
        var badObserver = new Observer<string>(new object(), async _ =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Test exception");
        });

        // Good observer
        var goodObserver = new Observer<string>(new object(), async _ =>
        {
            goodObserverCalled = true;
            await Task.Delay(10);
        });

        // Act
        badObserver.Subscribe(subject);
        goodObserver.Subscribe(subject);

        var exception = Record.Exception(() => subject.Notify("test"));

        // Assert
        Assert.Null(exception); // Subject should swallow observer exceptions
        Assert.True(goodObserverCalled);
    }

    [Fact]
    public void Subject_ShouldNotNotify_AfterDisposal()
    {
        // Arrange
        var invoked = false;
        var subject = new Subject<string>();
        var observer = new Observer<string>(new object(), async _ =>
        {
            invoked = true;
            await Task.Delay(10);
        });

        observer.Subscribe(subject);

        // Act
        subject.Dispose();
        subject.Notify("test");

        // Assert
        Assert.False(invoked);
    }
}

