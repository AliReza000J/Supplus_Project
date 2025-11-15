namespace AccountManagement.Domain.TokenAgg
{
    public interface ITokenRepository
    {
            void Add(Token token);
            Token GetByRefreshToken(string refreshToken);
            Token GetByAccessToken(string accessToken);
            void Revoke(Token token);
            bool IsRevoked(string accessToken);
    }
}