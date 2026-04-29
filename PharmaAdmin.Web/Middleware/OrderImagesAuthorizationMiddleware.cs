using _0_Framework.Application;
using AccountManagement.Domain.RoleAgg;
using System.Security.Claims;

namespace PharmaAdmin.Web.Middleware
{
    public class OrderImagesAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _protectedPath;

        public OrderImagesAuthorizationMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _protectedPath = Path.Combine(env.WebRootPath, "Pictures", "Orders");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHelper = context.RequestServices.GetRequiredService<IAuthHelper>();
            var requestPath = context.Request.Path.Value;

            if (!string.IsNullOrWhiteSpace(requestPath) &&
                requestPath.StartsWith("/Pictures/Orders", System.StringComparison.OrdinalIgnoreCase))
            {
                ClaimsPrincipal user = context.User;

                if (user == null || !authHelper.IsAuthenticated())
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("ابتدا وارد حساب کاربری شوید."));
                    return;
                }

                var roleClaim = authHelper.CurrentAccountInfo().RoleId;
                if (roleClaim <= 0 || !(roleClaim == Roles.SuperAdmin || roleClaim == Roles.Admin || roleClaim == Roles.Support))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("شما مجوز دسترسی به این بخش را ندارید."));
                    return;
                }

                var filePath = Path.Combine(_protectedPath, requestPath.Substring("/Pictures/Orders".Length).TrimStart('/', '\\'));

                if (!File.Exists(filePath))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("File not found");
                    return;
                }

                var ext = Path.GetExtension(filePath).ToLower();
                var contentType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream"
                };

                context.Response.ContentType = contentType;
                await context.Response.SendFileAsync(filePath);
                return;
            }

            await _next(context);
        }
    }

}
