using _0_Framework.Application;
using Microsoft.Extensions.Caching.Memory;

namespace PharmaShared.Web.Services
{
    public class OtpRateLimiter : IOtpRateLimiter
    {
        private readonly IMemoryCache _cache;
        private const int MaxAttempts = 3;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

        private class Counter
        {
            public int Value;
        }

        public OtpRateLimiter(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool CanSend(string mobile)
        {
            var key = $"otp-rate:{mobile}";

            var counter = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Window;
                return new Counter();
            });

            var currentCount = Interlocked.Increment(ref counter!.Value);
            return currentCount <= MaxAttempts;
        }
    }

}
