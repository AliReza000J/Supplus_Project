using _0_Framework.Application;
using Microsoft.Extensions.Caching.Memory;

namespace PharmaAdmin.Web.Services
{
    public class AdminSmsRateLimiter : IAdminSmsRateLimiter
    {
        private readonly IMemoryCache _cache;

        private const int MaxAttempts = 5;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(30);

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
                entry.AbsoluteExpirationRelativeToNow = Window;
                return 0;
            });

            if (adminCount >= MaxAttempts)
                return false;

            _cache.Set(orderKey, true, TimeSpan.FromMinutes(2));
            _cache.Set(adminKey, adminCount + 1, Window);

            return true;
        }
    }
}
