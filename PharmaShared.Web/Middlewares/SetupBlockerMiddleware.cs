using AccountManagement.Infrastructure.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PharmaShared.Web.Middlewares
{
    public class SetupBlockerMiddleware
    {
        private readonly RequestDelegate _next;

        public SetupBlockerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AccountContext dbContext)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            if (path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/lib") || path.StartsWith("/images"))
            {
                await _next(context);
                return;
            }

            bool isInstallPath = path.StartsWith("/install");

            if (!SetupState.IsInitialized)
            {
                var admin = await dbContext.Accounts.FirstOrDefaultAsync(a => a.RoleId == 1);
                SetupState.IsSetupCompleted = admin != null && admin.IsProfileCompleted;
                SetupState.IsInitialized = true;
            }

            if (SetupState.IsSetupCompleted && isInstallPath)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            if (!SetupState.IsSetupCompleted && !isInstallPath)
            {
                context.Response.Redirect("/install");
                return;
            }

            await _next(context);
        }

    }


    public static class SetupState
    {
        public static bool IsSetupCompleted { get; set; } = false;
        public static bool IsInitialized { get; set; } = false;
    }
}
