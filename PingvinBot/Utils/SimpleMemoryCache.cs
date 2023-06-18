using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace PingvinBot.Utils;

public abstract class SimpleMemoryCache
{
    private readonly MemoryCache _cache;

    public SimpleMemoryCache(int cacheSize)
    {
        _cache = new MemoryCache(new MemoryCacheOptions()
        {
            SizeLimit = cacheSize,
        });
    }

    public async Task<T?> LoadFromCacheAsync<T>(string key, Func<Task<T>> delegateFunction, TimeSpan duration, int size = 1)
    {
        if (_cache.TryGetValue(key, out T? value))
            return value;

        var loadedData = await delegateFunction();

        _cache.Set(key, loadedData, new MemoryCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = duration,
            Size = size,
        });

        return loadedData;
    }

    public void SetCache<T>(string key, T value, TimeSpan duration, int size = 1)
    {
        _cache.Set(key, value, new MemoryCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = duration,
            Size = size,
        });
    }

    public void SetCache<T>(string key, T value, TimeSpan absoluteDuration, TimeSpan slidingDuration, int size = 1)
    {
        _cache.Set(key, value, new MemoryCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = absoluteDuration,
            SlidingExpiration = slidingDuration,
            Size = size,
        });
    }

    public T? GetCache<T>(string key)
    {
        if (_cache.TryGetValue(key, out T? value))
            return value;

        return default;
    }

    public void FlushCache(string key)
    {
        _cache.Remove(key);
    }
}
