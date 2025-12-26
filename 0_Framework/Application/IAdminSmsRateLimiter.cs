namespace _0_Framework.Application
{
    public interface IAdminSmsRateLimiter
    {
        bool CanResend(long orderId, long adminId);
    }
}
