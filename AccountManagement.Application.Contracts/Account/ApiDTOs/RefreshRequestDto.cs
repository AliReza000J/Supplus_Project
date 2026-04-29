namespace AccountManagement.Application.Contracts.Account.ApiDTOs
{
    /// <summary>
    /// Represents the data required to refresh an authentication token.
    /// </summary>
    public class RefreshRequestDto
    {
        /// <summary>
        /// The refresh token received during the initial login.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
