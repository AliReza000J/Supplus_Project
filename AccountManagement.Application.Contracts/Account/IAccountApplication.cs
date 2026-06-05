using _0_Framework.Application;
using AccountManagement.Application.Contracts.Account.ApiDTOs;
using AccountManagement.Application.Contracts.Account.ApplicationDTOs;

namespace AccountManagement.Application.Contracts.Account
{
    public interface IAccountApplication
    {
        // ==========================================
        // Admin Panel (Cookie-Based)
        // ==========================================
        Task<OperationResult> LoginToAdminPanel(LoginViaUserPass command, CancellationToken cancellation);
        Task LogoutFromAdminPanel(CancellationToken cancellation = default);
        Task<OperationResult> CreateAccountByAdmin(CreateAccount command, CancellationToken cancellation);
        Task<OperationResult> EditAccountByAdmin(EditAccount command, CancellationToken cancellation); // all accounts - all info!
        Task<OperationResult> EditPersonalProfile(EditPersonalProfile command, CancellationToken cancellation); // by user himself!
        Task<OperationResult> EditSuperAdminProfile(EditSuperAdminProfile command, CancellationToken cancellation); // for admin himself!
        Task<OperationResult> Activate(long id, CancellationToken cancellation);
        Task<OperationResult> Deactivate(long id, CancellationToken cancellation);
        Task<EditAccount?> GetDetailsForAdmin(long id, CancellationToken cancellation);
        Task<List<AccountViewModel>> Search(AccountSearchModel searchModel, CancellationToken cancellation);
        Task<OperationResult> ResetPasswordByAdmin(long userId, string newPassword, CancellationToken cancellation); // by admin
        Task<OperationResult> SendPasswordResetLink(SendPasswordResetLinkDto command, CancellationToken cancellation); // for admin
        Task<OperationResult> ResetPasswordWithLink(ResetPasswordWithLinkDto command, CancellationToken cancellation); // for admin

        // ==========================================
        // Store API (JWT-Based)
        // ==========================================
        Task<OperationResult> SendLoginOtp(string mobile, CancellationToken cancellation);
        Task<(OperationResult Result, JwtResponseDto? Data)> LoginOrRegisterWithOtp(string mobile, string code, CancellationToken cancellation); // تایید OTP و صدور توکن
        Task<(OperationResult Result, JwtResponseDto? Data)> LoginToStoreViaUserPass(LoginToApiViaUserMobilePass command, CancellationToken cancellation);
        Task<OperationResult> LogoutFromStore(string token, CancellationToken cancellation);
        Task<(OperationResult Result, JwtResponseDto? Data)> RefreshToken(string refreshToken, CancellationToken cancellation);
        Task<OperationResult> SendResetPasswordOtp(string mobile, CancellationToken cancellation);
        Task<OperationResult> ResetPasswordWithOtp(ResetPasswordWithOtpApi command, CancellationToken cancellation);

        // Profile Management (API)
        Task<OperationResult> UpdateProfileInfo(UpdateGeneralInfo command, CancellationToken cancellation);
        Task<OperationResult> RequestMobileChangeOtp(long userId, string newMobile, CancellationToken cancellation);
        Task<OperationResult> ChangeMobileApi(ChangeMobileDto command, CancellationToken cancellation);
        Task<OperationResult> SetPassword(SetPassword command, long userId, CancellationToken cancellation);

        // ==========================================
        // Shared (Admin & Store)
        // ==========================================
        Task<AccountViewModel> GetAccountDetails(long id, CancellationToken cancellation);
        Task<long> GetRoleIdByUsername(string username, CancellationToken cancellation);
        Task<bool> IsMobileExist(string mobile, CancellationToken cancellation);
        Task<OperationResult> ChangePassword(ChangePassword command, long userId, CancellationToken cancellation);
        Task<List<AccountViewModel>> GetAccounts(CancellationToken cancellation);
        Task<bool> NeedsSetup(CancellationToken cancellation);
        Task<OperationResult> Setup(SetupSuperAdminProfile command, CancellationToken cancellation);
    }

}
