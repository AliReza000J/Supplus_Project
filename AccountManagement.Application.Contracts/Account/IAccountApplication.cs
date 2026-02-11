using _0_Framework.Application;

namespace AccountManagement.Application.Contracts.Account
{
    public interface IAccountApplication
    {
        Task<AccountViewModel> GetAccountBy(long id, CancellationToken cancellation);
        Task<long> GetRoleIdByUsername(string username, CancellationToken cancellation);
        Task<OperationResult> Register(RegisterAccount command, CancellationToken cancellation);
        Task<OperationResult> RegisterByMobile(RegisterByMobile command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditAccount command, long userId, CancellationToken cancellation);
        Task<OperationResult> ChangePassword(ChangePassword command, long userId, CancellationToken cancellation);
        Task<OperationResult> Login(Login command, CancellationToken cancellation); // Admin panel - Cookie-based
        Task Logout(CancellationToken cancellation = default); // Admin panel - Cookie-based

        //

        Task<(OperationResult, JwtResponseDto? Data)> LoginWithJwt(Login command, CancellationToken cancellation); // Shop - JWT-based
        Task<(OperationResult, JwtResponseDto? Data)> LoginByMobileWithJwt(string mobile, CancellationToken cancellation); // Shop - JWT-based via mobile
        Task<OperationResult> LogoutWithJwt(string token, CancellationToken cancellation); // JWT-based logout
        Task<(OperationResult, JwtResponseDto? Data)> RefreshToken(string refreshToken, CancellationToken cancellation);
        Task<OperationResult> CompleteProfile(CompleteProfile command, CancellationToken cancellation);
        //

        Task<OperationResult> Deactivate(long id, CancellationToken cancellation);
        Task<OperationResult> Activate(long id, CancellationToken cancellation);
        //

        Task<EditAccount?> GetDetails(long id, CancellationToken cancellation);
        Task<List<AccountViewModel>> Search(AccountSearchModel searchModel, CancellationToken cancellation);
        Task<List<AccountViewModel>> GetAccounts(CancellationToken cancellation);
        //
        Task<bool> ExistsAccountByMobile(string mobile, CancellationToken cancellation);
    }
}
