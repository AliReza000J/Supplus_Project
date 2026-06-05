namespace _0_Framework.Application
{
    public interface IActionRateLimiter
    {
        bool CheckLimit(string actionName, string identifier, int maxRequests, TimeSpan timeWindow);
    }
}
