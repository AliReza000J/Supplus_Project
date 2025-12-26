using _0_Framework.Domain;
using AccountManagement.Application.Contracts.Account;

namespace AccountManagement.Domain.AccountAgg
{
    public interface IAccountRepository : IRepository<long, Account>
    {
        Task<Account?> GetByUsernameAsync(string username);
        Task<Account?> GetAccountByMobile(string mobile);
        Task<bool> ExistsAsync(string username, string mobile);
        Task<EditAccount> GetDetailsAsync(long id);
        Task<List<AccountViewModel>> GetAccountsAsync();
        Task<List<AccountViewModel>> SearchAsync(AccountSearchModel searchModel);
    }
}
