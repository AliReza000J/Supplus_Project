using AccountManagement.Domain.TokenAgg;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.Infrastructure.EFCore.Repository
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AccountContext _context;
        public TokenRepository(AccountContext context) => _context = context;

        public async Task AddAsync(Token token)
        {
            await _context.Tokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<Token?> GetByRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return null;

            return await _context.Tokens.SingleOrDefaultAsync
                (t => t.RefreshToken == refreshToken && !t.IsRevoked);
        }

        public async Task<Token?> GetByAccessTokenAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) return null;
            return await _context.Tokens.SingleOrDefaultAsync
                (t => t.AccessToken == accessToken && !t.IsRevoked);
        }

        public async Task RevokeAsync(Token token)
        {
            if (token == null) return;
            token.Revoke();
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsRevokedAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) return false;

            var t = await _context.Tokens.SingleOrDefaultAsync
                (x => x.AccessToken == accessToken);

            return t == null ? false : t.IsRevoked;
        }

        public bool IsRevoked(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) return false;

            var t = _context.Tokens.SingleOrDefault(x => x.AccessToken == accessToken);

            return t == null ? false : t.IsRevoked;
        }
    }
}
