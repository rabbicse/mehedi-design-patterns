using Mehedi.Patterns.Observer.Asynchronous;

namespace ObserverTests;


public class AsyncObserverTests
{
    [Fact]
    public async Task Observer_ShouldInvokeAction_WhenNotified()
    {
        // Arrange
        var invoked = false;
        var observer = new AsyncObserver<string>(new object(), async _ =>
        {
            invoked = true;
            await Task.Delay(10);
        });

        var subject = new AsyncSubject<string>();

        // Act
        observer.Subscribe(subject);
        await subject.NotifyAsync("test");

        // Assert
        Assert.True(invoked);
    }

    [Fact]
    public void Observer_ShouldThrow_WhenCreatedWithNullSender()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AsyncObserver<string>(null, async _ => await Task.CompletedTask));
    }

    [Fact]
    public void Observer_ShouldThrow_WhenCreatedWithNullAction()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AsyncObserver<string>(new object(), null));
    }

    [Fact]
    public async Task Observer_ShouldNotInvoke_AfterUnsubscribe()
    {
        // Arrange
        var invoked = false;
        var observer = new AsyncObserver<string>(new object(), async _ =>
        {
            invoked = true;
            await Task.Delay(10);
        });

        var subject = new AsyncSubject<string>();
        observer.Subscribe(subject);

        // Act
        observer.Unsubscribe();
        await subject.NotifyAsync("test");

        // Assert
        Assert.False(invoked);
    }
}
