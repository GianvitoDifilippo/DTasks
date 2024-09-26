﻿using DTasks.Serialization;
using DTasks.Storage;

namespace DTasks.Hosting;

public abstract class BinaryDTaskHost<TFlowId, TStack, THeap> : DTaskHost<TFlowId>
    where TFlowId : notnull
    where TStack : IFlowStack
{
    protected abstract IDTaskStorage<TStack> Storage { get; }

    protected abstract IDTaskConverter<THeap> Converter { get; }

    protected abstract Task OnSuspendedAsync(TFlowId flowId, ISuspensionCallback callback, CancellationToken cancellationToken);

    protected abstract Task OnDelayAsync(TFlowId flowId, TimeSpan delay, CancellationToken cancellationToken);

    protected abstract Task OnYieldAsync(TFlowId flowId, CancellationToken cancellationToken);

    protected abstract Task OnCompletedAsync(TFlowId flowId, CancellationToken cancellationToken);

    protected abstract Task OnCompletedAsync<TResult>(TFlowId flowId, TResult result, CancellationToken cancellationToken);

    protected sealed override Task SuspendCoreAsync(TFlowId flowId, IDTaskScope scope, DTask.DAwaiter awaiter, CancellationToken cancellationToken)
    {
        FlowHandler handler = new(flowId, this);
        return handler.SuspendAsync(scope, awaiter, cancellationToken);
    }

    protected sealed override Task ResumeCoreAsync(TFlowId flowId, IDTaskScope scope, DTask resultTask, CancellationToken cancellationToken)
    {
        FlowHandler handler = new(flowId, this);
        return handler.ResumeAsync(scope, resultTask, cancellationToken);
    }

    private struct FlowHandler(TFlowId flowId, BinaryDTaskHost<TFlowId, TStack, THeap> host) : IStateHandler, ISuspensionHandler, ICompletionHandler
    {
        private TStack _stack;
        private THeap _heap;

        public async Task SuspendAsync(IDTaskScope scope, DTask.DAwaiter awaiter, CancellationToken cancellationToken)
        {
            _stack = host.Storage.CreateStack();
            _heap = host.Converter.CreateHeap(scope);

            awaiter.SaveState(ref this);

            ReadOnlyMemory<byte> bytes = host.Converter.SerializeHeap(ref _heap);
            _stack.Push(bytes);

            await host.Storage.SaveStackAsync(flowId, ref _stack, cancellationToken);
            await awaiter.SuspendAsync(ref this, cancellationToken);
        }

        public async Task ResumeAsync(IDTaskScope scope, DTask resultTask, CancellationToken cancellationToken)
        {
            _stack = await host.Storage.LoadStackAsync(flowId, cancellationToken);
            ReadOnlyMemory<byte> heapBytes = await _stack.PopAsync(cancellationToken);
            if (heapBytes.IsEmpty)
                throw new CorruptedDFlowException(flowId, $"Data relative to d-async flow '{flowId}' was missing or corrupted.");

            _heap = host.Converter.DeserializeHeap(flowId, scope, heapBytes.Span);

            DTask.DAwaiter awaiter = default;
            bool hasAwaiter = false;
            while (true)
            {
                ReadOnlyMemory<byte> stateMachineBytes = await _stack.PopAsync(cancellationToken);
                if (stateMachineBytes.IsEmpty)
                {
                    if (!hasAwaiter)
                        throw new CorruptedDFlowException(flowId, $"Data relative to d-async flow '{flowId}' was missing or corrupted.");
                    break;
                }

                resultTask = host.Converter.DeserializeStateMachine(flowId, ref _heap, stateMachineBytes.Span, resultTask);
                awaiter = resultTask.GetDAwaiter();

                if (!await awaiter.IsCompletedAsync())
                {
                    awaiter.SaveState(ref this);

                    ReadOnlyMemory<byte> bytes = host.Converter.SerializeHeap(ref _heap);
                    _stack.Push(bytes);

                    await host.Storage.SaveStackAsync(flowId, ref _stack, cancellationToken);
                    await awaiter.SuspendAsync(ref this, cancellationToken);
                    return;
                }

                hasAwaiter = true;
            }

            await awaiter.CompleteAsync(ref this, cancellationToken);
        }

        void IStateHandler.SaveStateMachine<TStateMachine>(ref TStateMachine stateMachine, IStateMachineInfo info)
        {
            ReadOnlyMemory<byte> bytes = host.Converter.SerializeStateMachine(ref _heap, ref stateMachine, info);
            _stack.Push(bytes);
        }

        readonly Task ISuspensionHandler.OnDelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            return host.OnDelayAsync(flowId, delay, cancellationToken);
        }

        readonly Task ISuspensionHandler.OnYieldAsync(CancellationToken cancellationToken)
        {
            return host.OnYieldAsync(flowId, cancellationToken);
        }

        readonly Task ISuspensionHandler.OnSuspendedAsync(ISuspensionCallback callback, CancellationToken cancellationToken)
        {
            return host.OnSuspendedAsync(flowId, callback, cancellationToken);
        }

        readonly Task ICompletionHandler.OnCompletedAsync(CancellationToken cancellationToken)
        {
            return host.OnCompletedAsync(flowId, cancellationToken);
        }

        readonly Task ICompletionHandler.OnCompletedAsync<TResult>(TResult result, CancellationToken cancellationToken)
        {
            return host.OnCompletedAsync(flowId, result, cancellationToken);
        }
    }
}
