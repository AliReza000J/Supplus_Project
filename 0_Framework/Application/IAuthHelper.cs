namespace _0_Framework.Application
{
    public interface IAuthHelper
    {
        Task SignIn(AuthViewModel account, bool rememberMe);
        bool IsAuthenticated();
        bool IsSuperAdmin();
        bool IsAdmin();
        Task SignOut();
        string CurrentAccountRole();
        //string CurrentAccountMobile();
        long CurrentUserId();
        List<int> GetPermissions();
        AuthViewModel CurrentAccountInfo();
    }
}
