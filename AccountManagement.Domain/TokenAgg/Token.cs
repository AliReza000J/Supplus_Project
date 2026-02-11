namespace AccountManagement.Domain.TokenAgg
{
    public class Token
    {
        public long Id { get; private set; }
        public long UserId { get; private set; }

        public string AccessToken { get; private set; }
        public DateTime AccessTokenExpiry { get; private set; }

        public string RefreshToken { get; private set; }
        public DateTime RefreshTokenExpiry { get; private set; }

        public bool IsRevoked { get; private set; }

        protected Token() { }

        public Token(long userId, string accessToken, DateTime accessExpiry, string refreshToken, DateTime refreshExpiry)
        {
            UserId = userId;
            AccessToken = accessToken;
            AccessTokenExpiry = accessExpiry;
            RefreshToken = refreshToken;
            RefreshTokenExpiry = refreshExpiry;
            IsRevoked = false;
        }

        public void Revoke()
        {
            IsRevoked = true;
        }

        public bool IsRefreshExpired() => DateTime.UtcNow >= RefreshTokenExpiry;
        public bool IsAccessExpired() => DateTime.UtcNow >= AccessTokenExpiry;
    }
}
