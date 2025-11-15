using _0_Framework.Application;

namespace AccountManagement.Application.Contracts.Account
{
    public interface IAccountApplication
    {
        AccountViewModel GetAccountBy(long id);
        long GetRoleIdByUsername(string username);
        OperationResult Register(RegisterAccount command);
        OperationResult Edit(EditAccount command, string userId);
        OperationResult ChangePassword(ChangePassword command, string userId);
        OperationResult Login(Login command); // Admin panel - Cookie-based
        void Logout(); // Admin panel - Cookie-based

        (OperationResult Result, JwtResponseDto Data) LoginWithJwt(Login command); // Shop - JWT-based
        OperationResult LogoutWithJwt(string token); // JWT-based logout
        (OperationResult Result, JwtResponseDto Data) RefreshToken(string refreshToken);

        OperationResult Delete(long id);
        OperationResult Activate(long id);

        EditAccount GetDetails(long id);
        List<AccountViewModel> Search(AccountSearchModel searchModel);
        List<AccountViewModel> GetAccounts();
    }
}
