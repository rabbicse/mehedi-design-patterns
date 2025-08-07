using Mehedi.Patterns.Observer.Asynchronous;

namespace ObserverTests;

public class AsyncObserverFactoryTests : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await AsyncObserverFactory.ShutdownAsync(); // Clean slate for each test
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
        Assert.True(invoked, "Handler was not invoked after registration");
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
        await AsyncObserverFactory.Instance.UnregisterHandlerAsync(testKey, sender);
        await AsyncObserverFactory.Instance.NotifyAsync(testKey, "test");

        // Assert
        Assert.False(invoked, "Handler was invoked after unregistration");
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
        Assert.True(stringInvoked, "String handler was not invoked");
        Assert.True(intInvoked, "Int handler was not invoked");
    }

    [Fact]
    public async Task UnregisterHandlerAsync_ShouldNotAffectOtherSenders()
    {
        // Arrange
        var sender1Invoked = false;
        var sender2Invoked = false;
        const string testKey = "testKey";
        var sender1 = new object();
        var sender2 = new object();

        AsyncObserverFactory.Instance.RegisterHandler<string>(testKey, sender1, async _ =>
        {
            sender1Invoked = true;
            await Task.Delay(10);
        });

        AsyncObserverFactory.Instance.RegisterHandler<string>(testKey, sender2, async _ =>
        {
            sender2Invoked = true;
            await Task.Delay(10);
        });

        // Act
        await AsyncObserverFactory.Instance.UnregisterHandlerAsync(testKey, sender1);
        await AsyncObserverFactory.Instance.NotifyAsync(testKey, "test");

        // Assert
        Assert.False(sender1Invoked, "Sender1's handler was invoked after unregistration");
        Assert.True(sender2Invoked, "Sender2's handler was not invoked");
    }

    [Fact]
    public async Task UnregisterAll_ShouldRemoveAllHandlers()
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
        await AsyncObserverFactory.Instance.UnregisterAllSubscriptionAsync();
        await AsyncObserverFactory.Instance.NotifyAsync(testKey, "test");

        // Assert
        Assert.False(invoked, "Handler was invoked after full unregistration");
    }

    [Fact]
    public void RegisterHandler_ShouldThrowOnNullArguments()
    {
        // Arrange
        const string testKey = "testKey";
        var sender = new object();
        Func<string, Task> handler = async _ => await Task.Delay(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            AsyncObserverFactory.Instance.RegisterHandler<string>(null, sender, handler));

        Assert.Throws<ArgumentNullException>(() =>
            AsyncObserverFactory.Instance.RegisterHandler<string>(testKey, null, handler));

        Assert.Throws<ArgumentNullException>(() =>
            AsyncObserverFactory.Instance.RegisterHandler<string>(testKey, sender, null));
    }
}