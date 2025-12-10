namespace _0_Framework.Application
{
    public interface IAuthHelper
    {
        Task SignIn(AuthViewModel account);
        bool IsAuthenticated();
        bool IsSuperAdmin();
        Task SignOut();
        string CurrentAccountRole();
        //string CurrentAccountMobile();
        long CurrentAccountId();
        List<int> GetPermissions();
        AuthViewModel CurrentAccountInfo();
    }
}
