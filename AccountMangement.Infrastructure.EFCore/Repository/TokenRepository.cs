using AccountManagement.Domain.TokenAgg;
using AccountManagement.Infrastructure.EFCore;

namespace AccountManagement.Infrastructure.EFCore.Repository
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AccountContext _context;
        public TokenRepository(AccountContext context) => _context = context;

        public void Add(Token token)
        {
            _context.Tokens.Add(token);
            _context.SaveChanges();
        }

        public Token GetByRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return null;
            return _context.Tokens.SingleOrDefault(t => t.RefreshToken == refreshToken && !t.IsRevoked);
        }

        public Token GetByAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) return null;
            return _context.Tokens.SingleOrDefault(t => t.AccessToken == accessToken && !t.IsRevoked);
        }

        public void Revoke(Token token)
        {
            if (token == null) return;
            token.Revoke();
            _context.SaveChanges();
        }

        public bool IsRevoked(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) return false;
            var t = _context.Tokens.SingleOrDefault(x => x.AccessToken == accessToken);
            return t == null ? false : t.IsRevoked;
        }
    }
}
