using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace _0_Framework.Application
{
    public class AuthHelper : IAuthHelper
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthHelper(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public void SignIn(AuthViewModel account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            var http = _contextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available.");

            var claims = new List<Claim>
            {
                new Claim("AccountId", account.Id.ToString()),
                new Claim("RoleId", account.RoleId.ToString()),
                new Claim(ClaimTypes.Name, account.Fullname ?? string.Empty),
                new Claim("Username", account.Username ?? string.Empty),
            };

            if (!string.IsNullOrWhiteSpace(account.Role))
                claims.Add(new Claim(ClaimTypes.Role, account.Role));

            if (account.Permissions != null && account.Permissions.Any())
            {
                // Add a claim per permission (integer code)
                foreach (var p in account.Permissions.Distinct())
                {
                    claims.Add(new Claim("Permission", p.ToString()));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(2)
            };

            http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public void SignOut()
        {
            var http = _contextAccessor.HttpContext;
            if (http == null) return;
            http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public bool IsAuthenticated()
        {
            var http = _contextAccessor.HttpContext;
            return http?.User?.Identity?.IsAuthenticated ?? false;
        }

        public long CurrentAccountId()
        {
            var http = _contextAccessor.HttpContext;
            if (http?.User == null || !http.User.Identity.IsAuthenticated) return 0;
            var claim = http.User.Claims.FirstOrDefault(x => x.Type == "AccountId");
            if (claim == null) return 0;
            return long.TryParse(claim.Value, out var id) ? id : 0;
        }

        public string CurrentAccountRole()
        {
            var http = _contextAccessor.HttpContext;
            if (http?.User == null || !http.User.Identity.IsAuthenticated) return string.Empty;
            //var claim = http.User.Claims.FirstOrDefault(x => x.Type == "RoleId" || x.Type == ClaimTypes.Role);
            var claim = http.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            return claim?.Value ?? string.Empty;
        }

        //public string CurrentAccountMobile()
        //{
        //    var http = _contextAccessor.HttpContext;
        //    if (http?.User == null || !http.User.Identity.IsAuthenticated) return string.Empty;
        //    var claim = http.User.Claims.FirstOrDefault(x => x.Type == "Mobile");
        //    return claim?.Value ?? string.Empty;
        //}

        public List<int> GetPermissions()
        {
            var http = _contextAccessor.HttpContext;
            if (http?.User == null || !http.User.Identity.IsAuthenticated) return new List<int>();
            var list = http.User.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => { int v; return int.TryParse(c.Value, out v) ? v : 0; })
                .Where(v => v > 0)
                .ToList();
            return list;
        }

        public AuthViewModel CurrentAccountInfo()
        {
            var http = _contextAccessor.HttpContext;
            if (http?.User == null || !http.User.Identity.IsAuthenticated) return null;

            var user = http.User;
            var id = CurrentAccountId();
            var roleIdClaim = user.Claims.FirstOrDefault(x => x.Type == "RoleId");
            long roleId = 0;
            if (roleIdClaim != null) long.TryParse(roleIdClaim.Value, out roleId);

            var fullname = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value ?? string.Empty;
            var username = user.Claims.FirstOrDefault(x => x.Type == "Username")?.Value ?? string.Empty;
            var role = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value ?? string.Empty;
            var permissions = GetPermissions();

            return new AuthViewModel(id, roleId, fullname, username, permissions) { Role = role };
        }

        public bool IsSuperAdmin()
        {
            var http = _contextAccessor.HttpContext;
            if (http?.User == null || !http.User.Identity.IsAuthenticated) return false;

            var user = http.User;
            var id = CurrentAccountId();
            var roleIdClaim = user.Claims.FirstOrDefault(x => x.Type == "RoleId");
            long roleId = 0;
            if (roleIdClaim != null) long.TryParse(roleIdClaim.Value, out roleId);
            var username = user.Claims.FirstOrDefault(x => x.Type == "Username")?.Value ?? string.Empty;

            if (username == "admin" && roleId == 1 && id == 1)
                return true;

            return false;
        }
    }
}
