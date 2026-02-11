namespace AccountManagement.Domain.TokenAgg
{
    public interface ITokenRepository
    {
        Task AddAsync(Token token, CancellationToken cancellation);
        Task<Token?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellation);
        Task<Token?> GetByAccessTokenAsync(string accessToken, CancellationToken cancellation);
        Task RevokeAsync(Token token, CancellationToken cancellation);
        Task<bool> IsRevokedAsync(string accessToken, CancellationToken cancellation);
        bool IsRevoked(string accessToken);
    }
}