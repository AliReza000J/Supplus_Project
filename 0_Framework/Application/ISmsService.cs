namespace _0_Framework.Application
{
    public interface ISmsService
    {
        Task<bool> SendOtpAsync(string mobile, string otpCode);
        Task<bool> SendPostTrackingAsync(string mobile, string trackingCode);
    }

}
