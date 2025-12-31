using _0_Framework.Application;
using System.Text;
using System.Text.Json;

namespace ServiceHost.Services
{
    public class SmsService : ISmsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<SmsService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public Task<bool> SendLoginOtpAsync(string mobile, string otpCode)
        {
            var templateId = _config.GetValue<int>("SmsSettings:Templates:LoginOtp");
            var paramsList = new[]
            {
            new VerifySendParameterModel
            { Name = "CODE", Value = otpCode }
        };

            return SendSmsInternalAsync(mobile, templateId, paramsList);
        }

        public Task<bool> SendPostTrackingAsync(string mobile, string trackingCode)
        {
            var templateId = _config.GetValue<int>("SmsSettings:Templates:PostTracking");
            var paramsList = new[]
            {
            new VerifySendParameterModel
            { Name = "TRACKING", Value = trackingCode }
        };

            return SendSmsInternalAsync(mobile, templateId, paramsList);
        }

        private async Task<bool> SendSmsInternalAsync(string mobile, int templateId, VerifySendParameterModel[] parameters)
        {
            var apiKey = _config["SmsSettings:ApiKey"];
            var baseUrl = _config["SmsSettings:BaseUrl"];

            var model = new VerifySendModel
            {
                Mobile = mobile,
                TemplateId = templateId,
                Parameters = parameters
            };

            try
            {
                var client = _httpClientFactory.CreateClient("SmsIr");
                client.DefaultRequestHeaders.Remove("x-api-key");
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var content = new StringContent(
                    JsonSerializer.Serialize(model),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(baseUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    _logger.LogError(
                        "SMS.ir failed | Mobile: {Mobile} | Template: {TemplateId} | Error: {Error}",
                        mobile, templateId, error);

                    throw new ApplicationException($"SMS sending failed: {error}");
                }

                _logger.LogInformation(
                    "SMS sent successfully | Mobile: {Mobile} | Template: {TemplateId}",
                    mobile, templateId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SMS.ir exception | Mobile: {Mobile} | Template: {TemplateId}",
                    mobile, templateId);

                return false;
            }
        }
    }
}
