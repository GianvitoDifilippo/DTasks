﻿using DTasks.Hosting;

namespace DTasks;

public class DTaskTests
{
    [Fact]
    public void CompletedDTask_ShouldBeCompletedHaveAResult()
    {
        // Arrange
        const int result = 42;

        // Act
        DTask<int> task = DTask.FromResult(result);

        // Assert
        task.Status.Should().Be(DTaskStatus.RanToCompletion);
        task.Result.Should().Be(result);
    }

    [Fact]
    public async Task DelayDTask_ShouldCallOnDelayAsyncWhenSuspended()
    {
        // Arrange
        TimeSpan delay = TimeSpan.FromSeconds(42);
        var handler = Substitute.For<ISuspensionHandler>();

        DTask sut = DTask.Delay(delay);

        // Act
        await sut.SuspendAsync(ref handler, CancellationToken.None);

        // Assert
        await handler.Received().OnDelayAsync(delay, CancellationToken.None);
    }

    [Fact]
    public async Task SuspendedDTask_ShouldCallOnSuspendedAsyncWhenSuspendedWithInterface()
    {
        // Arrange
        var callback = Substitute.For<ISuspensionCallback>();
        var handler = Substitute.For<ISuspensionHandler>();

        DTask sut = DTask.Factory.Suspend(callback);

        // Act
        await sut.SuspendAsync(ref handler, CancellationToken.None);

        // Assert
        await handler.Received().OnCallbackAsync(callback, CancellationToken.None);
    }

    [Fact]
    public async Task SuspendedDTaskOfTResult_ShouldCallOnSuspendedAsyncWhenSuspendedWithInterface()
    {
        // Arrange
        var callback = Substitute.For<ISuspensionCallback>();
        var handler = Substitute.For<ISuspensionHandler>();

        DTask<int> sut = DTask.Factory.Suspend<int>(callback);

        // Act
        await sut.SuspendAsync(ref handler, CancellationToken.None);

        // Assert
        await handler.Received().OnCallbackAsync(callback, CancellationToken.None);
    }

    [Fact]
    public async Task SuspendedDTask_ShouldCallOnSuspendedAsyncWhenSuspendedWithDelegate()
    {
        // Arrange
        object flowId = new object();
        var callback = Substitute.For<SuspensionCallback>();
        var handler = Substitute.For<ISuspensionHandler>();
        handler.OnCallbackAsync(Arg.Any<ISuspensionCallback>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(call => call.ArgAt<ISuspensionCallback>(0).OnSuspendedAsync(flowId));

        DTask sut = DTask.Factory.Suspend(callback);

        // Act
        await sut.SuspendAsync(ref handler, CancellationToken.None);

        // Assert
        await callback.Received().Invoke(flowId, CancellationToken.None);
    }

    [Fact]
    public async Task SuspendedDTaskOfTResult_ShouldCallOnSuspendedAsyncWhenSuspendedWithDelegate()
    {
        // Arrange
        object flowId = new object();
        var callback = Substitute.For<SuspensionCallback>();
        var handler = Substitute.For<ISuspensionHandler>();
        handler.OnCallbackAsync(Arg.Any<ISuspensionCallback>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(call => call.ArgAt<ISuspensionCallback>(0).OnSuspendedAsync(flowId));

        DTask<int> sut = DTask.Factory.Suspend<int>(callback);

        // Act
        await sut.SuspendAsync(ref handler, CancellationToken.None);

        // Assert
        await callback.Received().Invoke(flowId, CancellationToken.None);
    }

    [Fact]
    public async Task YieldDTask_ShouldCallOnDelayWhenSuspended()
    {
        // Arrange
        var handler = Substitute.For<ISuspensionHandler>();

        DTask sut = DTask.Yield();

        // Act
        await sut.SuspendAsync(ref handler, CancellationToken.None);

        // Assert
        await handler.Received().OnYieldAsync(CancellationToken.None);
    }
}
