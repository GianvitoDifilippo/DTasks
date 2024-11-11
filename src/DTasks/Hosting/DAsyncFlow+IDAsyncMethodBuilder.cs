﻿using DTasks.CompilerServices;
using System.Diagnostics;

namespace DTasks.Hosting;

internal partial class DAsyncFlow : IDAsyncMethodBuilder
{
    void IDAsyncMethodBuilder.AwaitOnCompleted<TAwaiter>(ref TAwaiter awaiter)
    {
        if (awaiter is IDAsyncAwaiter)
        {
            AwaitContinue(ref awaiter);
        }
        else
        {
            var self = this;
            _builder.AwaitOnCompleted(ref awaiter, ref self);
        }
    }

    void IDAsyncMethodBuilder.AwaitUnsafeOnCompleted<TAwaiter>(ref TAwaiter awaiter)
    {
        if (awaiter is IDAsyncAwaiter)
        {
            AwaitContinue(ref awaiter);
        }
        else
        {
            var self = this;
            _builder.AwaitUnsafeOnCompleted(ref awaiter, ref self);
        }
    }

    private void AwaitContinue<TAwaiter>(ref TAwaiter awaiter)
    {
        Debug.Assert(awaiter is IDAsyncAwaiter);

        if (((IDAsyncAwaiter)awaiter).IsCompleted)
        {
            // To avoid possible stack dives, invoke the continuation asynchronously.
            // Awaiting a completed Task gets the job done and saves us the bother of flowing the execution context, as the state machine box takes care of that.

            Await(Task.CompletedTask);
        }
        else
        {
            _suspendingAwaiterOrType = typeof(TAwaiter).IsValueType
                ? typeof(TAwaiter)
                : awaiter;

            ((IDAsyncAwaiter)awaiter).Continue(this);
        }
    }

    void IDAsyncMethodBuilder.SetResult()
    {
        Resume();
    }

    void IDAsyncMethodBuilder.SetResult<TResult>(TResult result)
    {
        Resume(result);
    }

    void IDAsyncMethodBuilder.SetException(Exception exception)
    {
        Resume(exception);
    }

    void IDAsyncMethodBuilder.SetState<TStateMachine>(ref TStateMachine stateMachine)
    {
        Dehydrate(ref stateMachine);
    }
}
