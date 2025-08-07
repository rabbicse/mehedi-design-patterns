using Mehedi.Patterns.Observer.Synchronous;

namespace ObserverTests;

public class ObserverFactoryTests
{
    public void Initialize()
    {
        ObserverFactory.Shutdown();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public void Factory_ShouldRegisterAndNotifyHandler()
    {
        // Arrange
        var invoked = false;
        const string testKey = "testKey";
        var sender = new object();

        // Act
        ObserverFactory.Instance.RegisterHandler<string>(testKey, sender, async _ =>
        {
            invoked = true;
            await Task.Delay(10);
        });

        ObserverFactory.Instance.Notify(testKey, "test");

        // Assert
        Assert.True(invoked);
    }

    [Fact]
    public void Factory_ShouldUnregisterHandler()
    {
        // Arrange
        var invoked = false;
        const string testKey = "testKey";
        var sender = new object();

        ObserverFactory.Instance.RegisterHandler<string>(testKey, sender, async _ =>
        {
            invoked = true;
            await Task.Delay(10);
        });

        // Act
        ObserverFactory.Instance.UnregisterHandler(testKey, sender);
        ObserverFactory.Instance.Notify(testKey, "test");

        // Assert
        Assert.False(invoked);
    }

    [Fact]
    public void Factory_ShouldHandleMultipleTypes()
    {
        // Arrange
        var stringInvoked = false;
        var intInvoked = false;
        const string stringKey = "stringKey";
        const string intKey = "intKey";
        var sender = new object();

        // Act
        ObserverFactory.Instance.RegisterHandler<string>(stringKey, sender, async _ =>
        {
            stringInvoked = true;
            await Task.Delay(10);
        });

        ObserverFactory.Instance.RegisterHandler<int>(intKey, sender, async _ =>
        {
            intInvoked = true;
            await Task.Delay(10);
        });

        ObserverFactory.Instance.Notify(stringKey, "test");
        ObserverFactory.Instance.Notify(intKey, 42);

        // Assert
        Assert.True(stringInvoked);
        Assert.True(intInvoked);
    }
}
