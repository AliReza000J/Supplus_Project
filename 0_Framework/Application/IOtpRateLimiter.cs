namespace _0_Framework.Application
{
    public interface IOtpRateLimiter
    {
        bool CanSend(string mobile);
    }
}
