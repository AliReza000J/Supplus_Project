namespace _0_Framework.Application
{
    public interface IAuthHelper
    {
        void SignIn(AuthViewModel account);
        bool IsAuthenticated();
        bool IsAdmin();
        void SignOut();
        string CurrentAccountRole();
        //string CurrentAccountMobile();
        long CurrentAccountId();
        List<int> GetPermissions();
        AuthViewModel CurrentAccountInfo();
    }
}
