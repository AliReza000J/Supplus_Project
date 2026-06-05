using _0_Framework.Application;
using Microsoft.Extensions.Caching.Memory;

namespace PharmaAdmin.Web.Services
{
    public class AdminSmsRateLimiter : IAdminSmsRateLimiter
    {
        private readonly IMemoryCache _cache;

        private const int MaxAttempts = 5;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan OrderCooldown = TimeSpan.FromMinutes(2);

        private class Counter { public int Value = 0; }

        public AdminSmsRateLimiter(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool CanResend(long orderId, long adminId)
        {
            var orderKey = $"admin-sms:order:{orderId}";
            var adminKey = $"admin-sms:admin:{adminId}";

            if (_cache.TryGetValue(orderKey, out _))
                return false;

            var adminCounter = _cache.GetOrCreate(adminKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Window;
                return new Counter();
            });

            if (adminCounter == null) return false;

            var currentCount = Interlocked.Increment(ref adminCounter!.Value);

            if (currentCount > MaxAttempts)
                return false;

            _cache.Set(orderKey, true, OrderCooldown);

            return true;
        }
    }
}
