using _0_Framework.Application;

namespace AccountManagement.Application.Contracts.Account
{
    public interface IAccountApplication
    {
        Task<AccountViewModel> GetAccountBy(long id);
        Task<long> GetRoleIdByUsername(string username);
        Task<OperationResult> Register(RegisterAccount command);
        Task<OperationResult> Edit(EditAccount command, string userId);
        Task<OperationResult> ChangePassword(ChangePassword command, string userId);
        Task<OperationResult> Login(Login command); // Admin panel - Cookie-based
        Task Logout(); // Admin panel - Cookie-based

        //

        Task<(OperationResult, JwtResponseDto? Data)> LoginWithJwt(Login command); // Shop - JWT-based
        Task<OperationResult> LogoutWithJwt(string token); // JWT-based logout
        Task<(OperationResult, JwtResponseDto? Data)> RefreshToken(string refreshToken);

        //

        Task<OperationResult> Delete(long id);
        Task<OperationResult> Activate(long id);
        //

        Task<EditAccount> GetDetails(long id);
        Task<List<AccountViewModel>> Search(AccountSearchModel searchModel);
        Task<List<AccountViewModel>> GetAccounts();
    }
}
