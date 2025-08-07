using Mehedi.Patterns.Observer.Synchronous;

namespace ObserverTests;

public class ObserverTests
{
    [Fact]
    public void Observer_ShouldInvokeAction_WhenNotified()
    {
        // Arrange
        var invoked = false;
        var observer = new Observer<string>(new object(), async _ =>
        {
            invoked = true;
            await Task.Delay(10);
        });

        var subject = new Subject<string>();

        // Act
        observer.Subscribe(subject);
        subject.Notify("test");

        // Assert
        Assert.True(invoked);
    }

    [Fact]
    public void Observer_ShouldThrow_WhenCreatedWithNullSender()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Observer<string>(null, async _ => await Task.CompletedTask));
    }

    [Fact]
    public void Observer_ShouldThrow_WhenCreatedWithNullAction()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Observer<string>(new object(), null));
    }

    [Fact]
    public void Observer_ShouldNotInvoke_AfterUnsubscribe()
    {
        // Arrange
        var invoked = false;
        var observer = new Observer<string>(new object(), async _ =>
        {
            invoked = true;
            await Task.Delay(10);
        });

        var subject = new Subject<string>();
        observer.Subscribe(subject);

        // Act
        observer.Unsubscribe();
        subject.Notify("test");

        // Assert
        Assert.False(invoked);
    }
}