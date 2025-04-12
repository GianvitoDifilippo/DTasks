﻿using System.Net.Http.Json;
using DTasks.AspNetCore.Infrastructure.Http;
using DTasks.Infrastructure.Marshaling;
using Microsoft.Extensions.DependencyInjection;

namespace DTasks.AspNetCore;

internal sealed class WebhookDAsyncContinuation(IHttpClientFactory httpClientFactory, Uri callbackAddress) : IDAsyncContinuation
{
    public async Task OnSucceedAsync(DAsyncId flowId, CancellationToken cancellationToken = default)
    {
        using HttpClient http = httpClientFactory.CreateClient();
        await http.PostAsJsonAsync(callbackAddress, new
        {
            operationId = flowId
        }, cancellationToken);
    }

    public async Task OnSucceedAsync<TResult>(DAsyncId flowId, TResult result, CancellationToken cancellationToken = default)
    {
        using HttpClient http = httpClientFactory.CreateClient();
        await http.PostAsJsonAsync(callbackAddress, new
        {
            operationId = flowId,
            result
        }, cancellationToken);
    }

    public Task OnFailAsync(DAsyncId flowId, Exception exception, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task OnCancelAsync(DAsyncId flowId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public static TypedInstance<object> CreateMemento(Uri callbackAddress)
    {
        return new Memento(callbackAddress);
    }

    private sealed class Memento(Uri callbackAddress) : IDAsyncContinuationMemento
    {
        public Uri CallbackAddress { get; } = callbackAddress;
        
        public IDAsyncContinuation Restore(IServiceProvider services)
        {
            return new WebhookDAsyncContinuation(
                services.GetRequiredService<IHttpClientFactory>(),
                CallbackAddress);
        }
    }
}
