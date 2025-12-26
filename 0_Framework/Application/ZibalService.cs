using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace _0_Framework.Application
{
    public class ZibalService : IZibalService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient; 
        private readonly ILogger<ZibalService> _logger;

        public ZibalService(IConfiguration config, HttpClient httpClient, ILogger<ZibalService> logger)
        {
            _config = config;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(long trackId, string message)> RequestPayment(double amount, string orderId)
        {
            var settings = _config.GetSection("ZibalSettings");
            var amountInRial = Convert.ToInt64(amount * 10);

            var body = new
            {
                merchant = settings["Merchant"],
                amount = amountInRial,
                callbackUrl = settings["CallbackUrl"],
                orderId,
                description = $"Payment for Order {orderId}"
            };

            _logger.LogInformation("Zibal Payment Request started for OrderId: {OrderId}, Amount: {Amount} Rial", orderId, amountInRial);

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{settings["ApiUrl"]}request", body);
                var result = await response.Content.ReadFromJsonAsync<ZibalRequestResponse>();

                if (result?.Result == 100)
                {
                    var trackId = result.TrackId ?? 0;
                    _logger.LogInformation("Zibal Request Successful. TrackId: {TrackId}", trackId);
                    return (trackId, "Success");
                }

                _logger.LogWarning("Zibal Request Failed. Result: {Result}, Message: {Message}", result?.Result, result?.Message);
                return (0, result?.Message ?? "Error connecting to Zibal");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during Zibal RequestPayment for OrderId: {OrderId}", orderId);
                return (0, "Internal error in payment request");
            }
        }

        public async Task<(int result, double amount, long refNumber, string message)> VerifyPayment(long trackId)
        {
            var settings = _config.GetSection("ZibalSettings");
            var body = new { merchant = settings["Merchant"], trackId };

            _logger.LogInformation("--- PAYMENT VERIFICATION STARTED ---");
            _logger.LogInformation("Time: {Time}, TrackId: {TrackId}", DateTime.Now, trackId);

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{settings["ApiUrl"]}verify", body);

                var rawResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Zibal Raw Response: {RawResponse}", rawResponse);

                var result = await response.Content.ReadFromJsonAsync<ZibalVerifyResponse>();

                if (result == null)
                {
                    _logger.LogError("CRITICAL: Zibal returned empty response for TrackId: {TrackId}", trackId);
                    return (-1, 0, 0, "No response from gateway");
                }

                if (result.Result == 100 || result.Result == 201)
                {
                    _logger.LogInformation("SUCCESS: Payment verified for TrackId: {TrackId}. RefNumber: {Ref}",
                        trackId, result.RefNumber);
                }
                else
                {
                    _logger.LogWarning("FAILED: Payment verification failed. Code: {Code}, Message: {Msg}",
                        result.Result, result.Message);
                }

                return (result.Result, (result.Amount ?? 0) / 10.0, result.RefNumber ?? trackId, result.Message ?? "");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "FATAL ERROR: Exception in VerifyPayment for TrackId: {TrackId}", trackId);
                return (-1, 0, 0, "Internal Server Error during verification");
            }
        }
    }
}