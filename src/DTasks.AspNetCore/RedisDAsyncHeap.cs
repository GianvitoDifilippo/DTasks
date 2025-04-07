using System.Text.Json;
using DTasks.Infrastructure;
using DTasks.Infrastructure.State;
using DTasks.Utils;
using StackExchange.Redis;

namespace DTasks.AspNetCore;

public class RedisDAsyncHeap(IDatabase database) : IDAsyncHeap
{
    // TODO: Marshal when marshaling interface/implementation is stable

    public async Task SaveAsync<TKey, TValue>(TKey key, TValue value, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        RedisKey redisKey = key.ToString();
        RedisValue redisValue = JsonSerializer.Serialize(value);
        await database.StringSetAsync(redisKey, redisValue);
    }

    public async Task<Option<TValue>> LoadAsync<TKey, TValue>(TKey key, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        RedisKey redisKey = key.ToString();
        RedisValue redisValue = await database.StringGetAsync(redisKey);
        if (redisValue.HasValue)
            return JsonSerializer.Deserialize<TValue>(redisValue.ToString())!;

        return Option<TValue>.None;
    }

    public async Task DeleteAsync<TKey>(TKey key, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        RedisKey redisKey = key.ToString();
        await database.KeyDeleteAsync(redisKey);
    }
}
