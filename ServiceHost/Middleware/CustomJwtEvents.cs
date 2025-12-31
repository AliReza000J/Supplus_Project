using _0_Framework.Application;
using AccountManagement.Domain.TokenAgg;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ServiceHost.Middleware
{
    public class CustomJwtEvents : JwtBearerEvents
    {
        public override Task Challenge(JwtBearerChallengeContext context)
        {
            if (context.Response.HasStarted)
                return Task.CompletedTask;

            context.HandleResponse();

            string message = context.Error switch
            {
                "revoked_token" => "توکن باطل شده است.",
                "invalid_token" => "توکن نامعتبر یا باطل شده است.",
                "expired_token" => "توکن منقضی شده است.",
                _ => "ابتدا وارد حساب کاربری شوید."
            };

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var result = ApiResponse<object>.Fail(message);
            return context.Response.WriteAsJsonAsync(result);
        }

        public override Task Forbidden(ForbiddenContext context)
        {
            if (context.Response.HasStarted)
                return Task.CompletedTask;

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var result = ApiResponse<object>.Fail("شما مجوز دسترسی به این بخش را ندارید.");
            return context.Response.WriteAsJsonAsync(result);
        }

        public override Task MessageReceived(MessageReceivedContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrWhiteSpace(token))
            {
                var repo = context.HttpContext.RequestServices.GetRequiredService<ITokenRepository>();
                if (repo != null && repo.IsRevoked(token))
                    context.Fail("revoked_token");
            }
            return Task.CompletedTask;
        }
    }
}
