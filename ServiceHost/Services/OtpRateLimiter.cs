using _0_Framework.Application;
using Microsoft.Extensions.Caching.Memory;

namespace ServiceHost.Services
{
    public class OtpRateLimiter : IOtpRateLimiter
    {
        private readonly IMemoryCache _cache;

        private const int MaxAttempts = 3;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

        public OtpRateLimiter(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool CanSend(string mobile)
        {
            var key = $"otp-rate:{mobile}";

            var count = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Window;
                return 0;
            });

            if (count >= MaxAttempts)
                return false;

            _cache.Set(key, count + 1, Window);
            return true;
        }
    }

}
