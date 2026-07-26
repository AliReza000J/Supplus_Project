using _0_Framework.Application;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.Infrastructure.EFCore
{
    public class TokenCleanupService : ITokenCleanupService
    {
        private readonly AccountContext _db;

        public TokenCleanupService(AccountContext db)
        {
            _db = db;
        }

        public async Task<TokenCleanupResult> CleanupAsync(int deleteAfterDays = 7)
        {
            var result = new TokenCleanupResult();
            var now = DateTime.UtcNow;

            // ------------------------------
            // Step 1: Mark AccessToken as expired (but NOT revoke the token)
            // ------------------------------
            var expiredAccessTokens = await _db.Tokens
                .Where(t => t.AccessTokenExpiry < now)
                .ToListAsync();

            result.ExpiredAccessTokens = expiredAccessTokens.Count;

            // ------------------------------
            // Step 2: Auto-Revoke Refresh Tokens
            // ------------------------------
            var expiredRefreshTokens = await _db.Tokens
                .Where(t => !t.IsRevoked && t.RefreshTokenExpiry < now)
                .ToListAsync();

            foreach (var t in expiredRefreshTokens)
                t.Revoke();

            result.RevokedRefreshTokens = expiredRefreshTokens.Count;

            await _db.SaveChangesAsync();

            // ------------------------------
            // Step 3: Delete revoked tokens older than X days
            // ------------------------------
            var olderThan = now.AddDays(-deleteAfterDays);

            var deleted = await _db.Database.ExecuteSqlInterpolatedAsync($@"
                    DELETE FROM Tokens
                    WHERE IsRevoked = 1
                      AND RefreshTokenExpiry < {olderThan}
                ");

            result.DeletedTokens = deleted;

            return result;
        }
    }
}
