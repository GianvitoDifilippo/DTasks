﻿using DTasks.Hosting;

namespace DTasks;

internal sealed class SuspendedDTask<TResult, TCallback>(TCallback callback) : DTask<TResult>
    where TCallback : ISuspensionCallback
{
    internal override DTaskStatus Status => DTaskStatus.Suspended;

    internal override TResult Result
    {
        get
        {
            InvalidStatus(expectedStatus: DTaskStatus.RanToCompletion);
            return default!;
        }
    }

    internal override Task<bool> UnderlyingTask => Task.FromResult(false);

    internal override Task SuspendAsync<THandler>(ref THandler handler, CancellationToken cancellationToken)
    {
        return handler.OnCallbackAsync(callback, cancellationToken);
    }
}

internal sealed class SuspendedDTask<TResult, TState, TCallback>(TState state, TCallback callback) : DTask<TResult>
    where TCallback : ISuspensionCallback<TState>
{
    internal override DTaskStatus Status => DTaskStatus.Suspended;

    internal override TResult Result
    {
        get
        {
            InvalidStatus(expectedStatus: DTaskStatus.RanToCompletion);
            return default!;
        }
    }

    internal override Task<bool> UnderlyingTask => Task.FromResult(false);

    internal override Task SuspendAsync<THandler>(ref THandler handler, CancellationToken cancellationToken)
    {
        return handler.OnCallbackAsync(state, callback, cancellationToken);
    }
}
