using _0_Framework.Application;
using Microsoft.Extensions.Caching.Memory;

namespace PharmaShared.Web.Services
{
    public class PasswordResetRateLimiter : IPasswordResetRateLimiter
    {
        private readonly IMemoryCache _cache;

        private const int MaxAttempts = 3;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

        public PasswordResetRateLimiter(IMemoryCache cache) => _cache = cache;

        public Task<bool> TryLimitAsync(string key, int maxRequests, TimeSpan period)
        {
            var entry = _cache.GetOrCreate(key, e =>
            {
                e.SlidingExpiration = period;
                return new RequestCounter { Count = 0 };
            });

            if (entry.Count >= maxRequests) return Task.FromResult(false);
            entry.Count++;
            return Task.FromResult(true);
        }

        private class RequestCounter { public int Count; }
    }

}
