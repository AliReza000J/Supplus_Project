using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace ServiceHost.Filters
{
    public class RateLimitAttribute : ActionFilterAttribute
    {
        public int Limit { get; set; } = 5;
        public int Seconds { get; set; } = 60;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cache = context.HttpContext.RequestServices
                .GetRequiredService<IMemoryCache>();

            var httpContext = context.HttpContext;

            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var key = $"rl:{context.ActionDescriptor.DisplayName}:{userId ?? ip}";

            var count = cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Seconds);
                return 0;
            });

            if (count >= Limit)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status429TooManyRequests);
                return;
            }

            cache.Set(key, count + 1, TimeSpan.FromSeconds(Seconds));
        }
    }
}
