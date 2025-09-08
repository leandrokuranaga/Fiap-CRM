using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Fiap.CRM.Infra.CacheService.Services;


[ExcludeFromCodeCoverage]
public sealed class CacheService(HybridCache cache, IConnectionMultiplexer redis, ILogger<CacheService> logger) : ICacheService
{
    private readonly HybridCache _cache = cache;
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly ILogger<CacheService> _logger = logger;

    private static readonly HybridCacheEntryOptions CACHE_OPTIONS = new()
    {
        Expiration = TimeSpan.FromMinutes(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };

    public async Task SetAsync(string cacheTag, object cacheValue)
    {
        _logger.LogInformation("Creating cache\n\tTag: {0}", cacheTag);

        if (cacheTag is null || cacheValue is null)
        {
            _logger.LogWarning("Cache not created {0} {1} cannot be null", nameof(cacheTag), nameof(cacheValue));
            return;
        }

        await _cache.SetAsync(cacheTag, cacheValue, CACHE_OPTIONS);
    }

    public async Task RemoveAsync(string cacheTag)
    {
        if (cacheTag is null)
            return;

        await _cache.RemoveAsync(cacheTag);

        var database = _redis.GetDatabase();

        foreach (var endpoint in _redis.GetEndPoints())
        {
            var server = _redis.GetServer(endpoint);

            var keys = server.KeysAsync(pattern: $"{cacheTag}*");

            await foreach (var key in keys)
            {
                _logger.LogInformation("Deleting key: {0}\n", key);

                var deletedStatus = await database.KeyDeleteAsync(key);

                if (deletedStatus)
                    _logger.LogInformation("Deleted Key: {0}", key);
                else
                    _logger.LogWarning("NOT Deleted Key: {0}", key);
            }
        }
    }

    public Task RemoveAsync(params string[] cacheTags)
    {
        var taskArray = cacheTags.Select(tag => RemoveAsync(tag));

        return Task.WhenAll(taskArray);
    }

    public async Task<TResponse?> GetAsync<TResponse>(string cacheTag)
    {
        if (cacheTag is null)
            return default;

        _logger.LogInformation("Retrieving Cache\n\tTag: {0}\n", cacheTag);


        var response = await _cache.GetOrCreateAsync(
            cacheTag,
            (token) => new ValueTask<TResponse>(default(TResponse)));

        var success = response is not null;
        _logger.LogInformation("Retrieving Cache\n\tTag: {0}\n\tSuccess: {1}\n", cacheTag, success);

        return response;
    }

    public async Task<TResponse?> GetOrSetAsync<TResponse>(string cacheTag, Func<TResponse?> fallbackFunc)
    {
        return await _cache.GetOrCreateAsync(
            cacheTag,
            (token) =>
            {
                var result = fallbackFunc();
                _logger.LogInformation("Cache miss for tag: {0}, executing fallback", cacheTag);
                return new ValueTask<TResponse>(result);
            },
            CACHE_OPTIONS);
    }

    public async Task<TResponse?> GetOrSetAsync<TResponse>(string cacheTag, Func<Task<TResponse>> fallbackFuncAsync)
    {
        return await _cache.GetOrCreateAsync(
            cacheTag,
            async (token) =>
            {
                var result = await fallbackFuncAsync();
                _logger.LogInformation("Cache miss for tag: {0}, executing async fallback", cacheTag);
                return result;
            },
            CACHE_OPTIONS);
    }
}