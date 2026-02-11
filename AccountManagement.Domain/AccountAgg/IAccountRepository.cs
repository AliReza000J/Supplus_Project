using _0_Framework.Domain;
using AccountManagement.Application.Contracts.Account;

namespace AccountManagement.Domain.AccountAgg
{
    public interface IAccountRepository : IRepository<long, Account>
    {
        Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellation);
        Task<Account?> GetAccountByMobile(string mobile, CancellationToken cancellation);
        Task<bool> ExistsAsync(string username, string mobile, CancellationToken cancellation);
        Task<EditAccount?> GetDetailsAsync(long id, CancellationToken cancellation);
        Task<List<AccountViewModel>> GetAccountsAsync(CancellationToken cancellation);
        Task<List<AccountViewModel>> SearchAsync(AccountSearchModel searchModel, CancellationToken cancellation);
    }
}
