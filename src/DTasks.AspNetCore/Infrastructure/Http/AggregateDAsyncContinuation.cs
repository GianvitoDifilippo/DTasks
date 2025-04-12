using DTasks.Infrastructure.Marshaling;

namespace DTasks.AspNetCore.Infrastructure.Http;

internal sealed class AggregateDAsyncContinuation : IDAsyncContinuation
{
    private readonly IDAsyncContinuation[] _callbacks;

    private AggregateDAsyncContinuation(IDAsyncContinuation[] callbacks)
    {
        // TODO: Support parallel execution when configured
        _callbacks = callbacks;
    }
    
    public async Task OnSucceedAsync(DAsyncId flowId, CancellationToken cancellationToken = default)
    {
        foreach (IDAsyncContinuation callback in _callbacks)
        {
            await callback.OnSucceedAsync(flowId, cancellationToken);
        }
    }

    public async Task OnSucceedAsync<TResult>(DAsyncId flowId, TResult result,
        CancellationToken cancellationToken = default)
    {
        foreach (IDAsyncContinuation callback in _callbacks)
        {
            await callback.OnSucceedAsync(flowId, result, cancellationToken);
        }
    }

    public async Task OnFailAsync(DAsyncId flowId, Exception exception, CancellationToken cancellationToken = default)
    {
        foreach (IDAsyncContinuation callback in _callbacks)
        {
            await callback.OnFailAsync(flowId, exception, cancellationToken);
        }
    }

    public async Task OnCancelAsync(DAsyncId flowId, CancellationToken cancellationToken = default)
    {
        foreach (IDAsyncContinuation callback in _callbacks)
        {
            await callback.OnCancelAsync(flowId, cancellationToken);
        }
    }

    public static TypedInstance<object> CreateMemento(TypedInstance<object>[] mementos) => new Memento(mementos);

    private sealed class Memento(TypedInstance<object>[] mementos) : IDAsyncContinuationMemento
    {
        // TODO: Remove public property and make type inspectable
        public TypedInstance<object>[] Mementos { get; } = mementos;

        public IDAsyncContinuation Restore(IServiceProvider services)
        {
            IDAsyncContinuation[] callbacks = new IDAsyncContinuation[Mementos.Length];

            for (int i = 0; i < Mementos.Length; i++)
            {
                var memento = (IDAsyncContinuationMemento)Mementos[i].Value;
                callbacks[i] = memento.Restore(services);
            }
            
            return new AggregateDAsyncContinuation(callbacks);
        }
    }
}