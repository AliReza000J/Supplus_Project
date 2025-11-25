namespace _0_Framework.Application
{
    public interface IAuthHelper
    {
        void SignIn(AuthViewModel account);
        bool IsAuthenticated();
        bool IsSuperAdmin();
        void SignOut();
        string CurrentAccountRole();
        //string CurrentAccountMobile();
        long CurrentAccountId();
        List<int> GetPermissions();
        AuthViewModel CurrentAccountInfo();
    }
}
