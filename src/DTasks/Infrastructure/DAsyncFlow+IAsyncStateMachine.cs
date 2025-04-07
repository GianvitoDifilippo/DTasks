﻿using DTasks.Utils;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DTasks.Infrastructure;

public sealed partial class DAsyncFlow : IAsyncStateMachine
{
    void IAsyncStateMachine.MoveNext()
    {
        try
        {
            switch (_state)
            {
                case FlowState.Running: // After awaiting a regular awaitable or a completed d-awaitable
                    Assert.NotNull(_stateMachine);

                    _stateMachine.MoveNext();
                    break;

                case FlowState.Dehydrating: // After awaiting _stateManager.DehydrateAsync
                    Assert.NotNull(_continuation);

                    GetVoidValueTaskResult();
                    _suspendingAwaiterOrType = null;
                    _state = FlowState.Running;
                    Consume(ref _continuation).Invoke(this);
                    break;

                case FlowState.Hydrating: // After awaiting _stateManager.HydrateAsync
                    (DAsyncId parentId, IDAsyncRunnable runnable) = GetLinkValueTaskResult();
                    if (parentId != default)
                    {
                        _parentId = parentId; // TODO: Remove this and implement a better solution. It is like this only to support handles
                    }

                    _state = FlowState.Running;
                    runnable.Run(this);
                    break;
                
                case FlowState.Suspending: // After awaiting a method that results in suspending the d-async flow
                    GetVoidTaskResult();
                    AwaitOnSuspend();
                    break;

                case FlowState.Returning: // After awaiting a method that results in completing the current run
                    GetVoidTaskResult();
                    _valueTaskSource.SetResult(default);
                    break;

                case FlowState.Aggregating: // After awaiting WhenAllAsync, WhenAnyAsync, etc
                    Assert.NotNull(_aggregateRunnable);

                    GetVoidTaskResult();
                    _state = FlowState.Running;
                    Consume(ref _aggregateRunnable).Run(this);
                    break;

                case FlowState.Awaiting:
                    Task? awaitedTask = Consume(ref _awaitedTask);
                    object? resultBuilder = Consume(ref _resultBuilder);

                    Assert.NotNull(awaitedTask);
                    Assert.Is<IDAsyncResultBuilder<Task>>(resultBuilder);

                    try
                    {
                        GetVoidTaskResult();
                        Unsafe.As<IDAsyncResultBuilder<Task>>(resultBuilder).SetResult(awaitedTask);
                    }
                    catch (Exception ex)
                    {
                        Unsafe.As<IDAsyncResultBuilder<Task>>(resultBuilder).SetException(ex);
                    }
                    break;

                default:
                    Debug.Fail($"Invalid state after await: '{_state}'.");
                    break;
            }
        }
        catch (Exception ex)
        {
            _valueTaskSource.SetException(ex);
        }
    }

    [ExcludeFromCodeCoverage]
    void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
    {
        ThrowHelper.ThrowIfNull(stateMachine);

        Debug.Fail("SetStateMachine should not be used.");
    }

    private enum FlowState
    {
        Idling, // In the pool
        Pending, // Leased, waiting for runnable
        Running, // Executing the runnable
        Dehydrating, // Awaiting DehydrateAsync
        Hydrating, // Awaiting HydrateAsync
        Suspending, // Awaiting suspension callback
        Returning, // Returning from the runnable
        Aggregating, // Running multiple aggregated runnables
        Awaiting // Awaiting a custom task
    }
}
