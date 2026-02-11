using AccountManagement.Domain.TokenAgg;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.Infrastructure.EFCore.Repository
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AccountContext _context;
        public TokenRepository(AccountContext context) => _context = context;

        public async Task AddAsync(Token token, CancellationToken cancellation)
        {
            await _context.Tokens.AddAsync(token, cancellation);
            await _context.SaveChangesAsync(cancellation);
        }

        public async Task<Token?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return null;

            return await _context.Tokens.SingleOrDefaultAsync
                (t => t.RefreshToken == refreshToken && !t.IsRevoked, cancellation);
        }

        public async Task<Token?> GetByAccessTokenAsync(string accessToken, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) return null;
            return await _context.Tokens.SingleOrDefaultAsync
                (t => t.AccessToken == accessToken && !t.IsRevoked, cancellation);
        }

        public async Task RevokeAsync(Token token, CancellationToken cancellation)
        {
            if (token == null) return;
            token.Revoke();
            await _context.SaveChangesAsync(cancellation);
        }

        public async Task<bool> IsRevokedAsync(string accessToken, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) return false;

            var t = await _context.Tokens.SingleOrDefaultAsync
                (x => x.AccessToken == accessToken, cancellation);

            return t != null && t.IsRevoked;
        }

        public bool IsRevoked(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) return false;

            var t = _context.Tokens.SingleOrDefault(x => x.AccessToken == accessToken);

            return t != null && t.IsRevoked;
        }
    }
}
