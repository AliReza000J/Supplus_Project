using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace _0_Framework.Application
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCacheService> _logger;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks
            = new();

        public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            long? size = null)
        {
            if (_cache.TryGetValue(key, out T cachedValue))
            {
                _logger.LogInformation("CACHE MISS → {0}", key);
                return cachedValue;
            }

            _logger.LogInformation("CACHE MISS → {0}", key);

            var myLock = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));

            await myLock.WaitAsync();
            try
            {
                if (_cache.TryGetValue(key, out cachedValue))
                    return cachedValue;

                T value = await factory();

                var options = new MemoryCacheEntryOptions();

                if (absoluteExpiration != null)
                    options.AbsoluteExpirationRelativeToNow = absoluteExpiration;

                if (slidingExpiration != null)
                    options.SlidingExpiration = slidingExpiration;

                if (size != null)
                    options.SetSize(size.Value);

                _cache.Set(key, value, options);

                return value;
            }
            finally
            {
                myLock.Release();
                _locks.TryRemove(key, out _);
            }
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }


}
