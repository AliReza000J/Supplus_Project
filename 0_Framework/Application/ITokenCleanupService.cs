namespace _0_Framework.Application
{
    public interface ITokenCleanupService
    {
        /// <summary>
        /// Revokes all expired tokens (access/refresh) and deletes old revoked tokens.
        /// </summary>
        Task<TokenCleanupResult> CleanupAsync(int deleteAfterDays = 7);
    }

    public class TokenCleanupResult
    {
        public int ExpiredAccessTokens { get; set; }
        public int RevokedRefreshTokens { get; set; }
        public int DeletedTokens { get; set; }
    }
}
