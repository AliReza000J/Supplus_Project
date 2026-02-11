using System.Security.Claims;

namespace _0_Framework.Application
{
    public interface IJwtService
    {
        string GenerateAccessToken(long userId, string username, long roleId, IEnumerable<int> permissions, int expireMinutes = 15);
        string GenerateRefreshToken();
        System.DateTime GetExpiration(string token);
        ClaimsPrincipal ValidateToken(string token);
    }
}
