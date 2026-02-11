using AccountManagement.Domain.RoleAgg;
using Hangfire.Dashboard;

namespace ServiceHost.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity?.IsAuthenticated == true
                && (
                    httpContext.User.IsInRole(Roles.GetRoleBy(1)) ||
                    httpContext.User.IsInRole(Roles.GetRoleBy(2))
                );
        }
    }
}
