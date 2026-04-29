namespace _0_Framework.Application
{
    public interface IPasswordResetRateLimiter
    {
        Task<bool> TryLimitAsync(string key, int maxRequests, TimeSpan period);
    }
}
