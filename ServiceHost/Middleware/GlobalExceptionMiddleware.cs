using _0_Framework.Application;
using System.Net;
using System.Text.Json;

namespace ServiceHost.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught by GlobalExceptionMiddleware.");

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Cannot write error response. Response has already started.");
                return;
            }

            var (statusCode, message) = MapException(ex);

            var response = ApiResponse<object>.Fail(message);

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }

        // الگوی استاندارد برای نگاشت خطاها
        private (HttpStatusCode statusCode, string message) MapException(Exception ex)
        {
            return ex switch
            {
                ArgumentNullException => (HttpStatusCode.BadRequest, "ورودی نامعتبر است."),
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "دسترسی غیرمجاز."),
                InvalidOperationException => (HttpStatusCode.Conflict, ex.Message),
                KeyNotFoundException => (HttpStatusCode.NotFound, "موردی یافت نشد."),
                _ => (HttpStatusCode.InternalServerError, "خطای غیرمنتظره‌ای رخ داد.")
            };
        }
    }
}
