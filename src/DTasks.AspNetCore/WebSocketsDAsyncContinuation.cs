﻿using DTasks.AspNetCore.Infrastructure.Http;
using DTasks.Infrastructure.Marshaling;
using Microsoft.Extensions.DependencyInjection;

namespace DTasks.AspNetCore;

internal sealed class WebSocketsDAsyncContinuation(IWebSocketHandler handler, string connectionId) : IDAsyncContinuation
{
    public Task OnSucceedAsync(DAsyncId flowId, CancellationToken cancellationToken = default)
    {
        var message = new
        {
            operationId = flowId
        };
        
        return handler.SendAsync(connectionId, message, cancellationToken);
    }

    public Task OnSucceedAsync<TResult>(DAsyncId flowId, TResult result, CancellationToken cancellationToken = default)
    {
        var message = new
        {
            operationId = flowId,
            result
        };
        
        return handler.SendAsync(connectionId, message, cancellationToken);
    }

    public Task OnFailAsync(DAsyncId flowId, Exception exception, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task OnCancelAsync(DAsyncId flowId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public static TypedInstance<object> CreateMemento(string connectionId)
    {
        return new Memento(connectionId);
    }
    
    private sealed class Memento(string connectionId) : IDAsyncContinuationMemento
    {
        public string ConnectionId { get; } = connectionId;
        
        public IDAsyncContinuation Restore(IServiceProvider services)
        {
            return new WebSocketsDAsyncContinuation(
                services.GetRequiredService<IWebSocketHandler>(),
                ConnectionId);
        }
    }
}
