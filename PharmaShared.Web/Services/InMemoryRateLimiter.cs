using _0_Framework.Application;
using Microsoft.Extensions.Caching.Memory;

namespace PharmaShared.Web.Services
{
    public class InMemoryRateLimiter : IActionRateLimiter
    {
        private readonly IMemoryCache _cache;

        public InMemoryRateLimiter(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool CheckLimit(string actionName, string identifier, int maxRequests, TimeSpan timeWindow)
        {
            var key = $"ratelimit:{actionName}:{identifier}";

            var counter = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = timeWindow;
                return new Counter();
            });

            var currentCount = Interlocked.Increment(ref counter!.Value);

            return currentCount <= maxRequests;
        }

        private class Counter
        {
            public int Value = 0;
        }
    }

}
