using _0_Framework.Application;
using Hangfire;

namespace PharmaAdmin.Web.Services
{
    public class PostTrackingSmsJob
    {
        private readonly ISmsService _smsService;

        public PostTrackingSmsJob(ISmsService smsService)
        {
            _smsService = smsService;
        }

        [AutomaticRetry(
            Attempts = 5,
            DelaysInSeconds = new[] { 60, 300, 600, 1800, 3600 },
            OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task SendAsync(string mobile, string trackingCode)
        {
            await _smsService.SendPostTrackingAsync(mobile, trackingCode);
        }
    }
}
