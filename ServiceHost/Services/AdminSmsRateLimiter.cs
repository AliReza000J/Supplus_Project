using _0_Framework.Application;
using Microsoft.Extensions.Caching.Memory;

namespace ServiceHost.Services
{
    public class AdminSmsRateLimiter : IAdminSmsRateLimiter
    {
        private readonly IMemoryCache _cache;

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

            var adminCount = _cache.GetOrCreate(adminKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return 0;
            });

            if (adminCount >= 5)
                return false;

            _cache.Set(orderKey, true, TimeSpan.FromMinutes(2));
            _cache.Set(adminKey, adminCount + 1, TimeSpan.FromHours(1));

            return true;
        }
    }
}
