using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace _0_Framework.Application
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks
            = new();

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
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
                Console.WriteLine($"CACHE HIT → {key}");
                return cachedValue;
            }

            Console.WriteLine($"CACHE MISS → {key}");

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
