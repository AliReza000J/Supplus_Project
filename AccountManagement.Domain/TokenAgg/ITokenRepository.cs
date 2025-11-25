namespace AccountManagement.Domain.TokenAgg
{
    public interface ITokenRepository
    {
        Task AddAsync(Token token);
        Task<Token?> GetByRefreshTokenAsync(string refreshToken);
        Task<Token?> GetByAccessTokenAsync(string accessToken);
        Task RevokeAsync(Token token);
        Task<bool> IsRevokedAsync(string accessToken);
        bool IsRevoked(string accessToken);
    }
}