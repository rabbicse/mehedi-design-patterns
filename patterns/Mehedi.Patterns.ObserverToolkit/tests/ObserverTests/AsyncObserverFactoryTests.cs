using Mehedi.Patterns.Observer.Asynchronous;

namespace ObserverTests;

public class AsyncObserverFactoryTests : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await AsyncObserverFactory.ShutdownAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Factory_ShouldRegisterAndNotifyHandler()
    {
        // Arrange
        var invoked = false;
        const string testKey = "testKey";
        var sender = new object();

        // Act
        AsyncObserverFactory.Instance.RegisterHandler<string>(testKey, sender, async _ =>
        {
            invoked = true;
            await Task.Delay(10);
        });

        await AsyncObserverFactory.Instance.NotifyAsync(testKey, "test");

        // Assert
        Assert.True(invoked);
    }

    [Fact]
    public async Task Factory_ShouldUnregisterHandler()
    {
        // Arrange
        var invoked = false;
        const string testKey = "testKey";
        var sender = new object();

        AsyncObserverFactory.Instance.RegisterHandler<string>(testKey, sender, async _ =>
        {
            invoked = true;
            await Task.Delay(10);
        });

        // Act
        AsyncObserverFactory.Instance.UnregisterHandler(testKey, sender);
        await AsyncObserverFactory.Instance.NotifyAsync(testKey, "test");

        // Assert
        Assert.False(invoked);
    }

    [Fact]
    public async Task Factory_ShouldHandleMultipleTypes()
    {
        // Arrange
        var stringInvoked = false;
        var intInvoked = false;
        const string stringKey = "stringKey";
        const string intKey = "intKey";
        var sender = new object();

        // Act
        AsyncObserverFactory.Instance.RegisterHandler<string>(stringKey, sender, async _ =>
        {
            stringInvoked = true;
            await Task.Delay(10);
        });

        AsyncObserverFactory.Instance.RegisterHandler<int>(intKey, sender, async _ =>
        {
            intInvoked = true;
            await Task.Delay(10);
        });

        await AsyncObserverFactory.Instance.NotifyAsync(stringKey, "test");
        await AsyncObserverFactory.Instance.NotifyAsync(intKey, 42);

        // Assert
        Assert.True(stringInvoked);
        Assert.True(intInvoked);
    }
}
