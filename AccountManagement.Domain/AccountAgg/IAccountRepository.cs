using _0_Framework.Domain;
using AccountManagement.Application.Contracts.Account.ApplicationDTOs;

namespace AccountManagement.Domain.AccountAgg
{
    public interface IAccountRepository : IRepository<long, Account>
    {
        Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellation);
        Task<Account?> GetByMobileAsync(string mobile, CancellationToken cancellation);
        Task<Account?> GetByUsernameOrMobileAsync(string usernameOrMobile, CancellationToken cancellation);

        Task<Account?> GetByEmailAsync(string email, CancellationToken cancellation);
        Task<EditAccount?> GetDetailsAsync(long id, CancellationToken cancellation);
        Task<List<AccountViewModel>> GetAccountsAsync(CancellationToken cancellation);
        Task<List<AccountViewModel>> SearchAsync(AccountSearchModel searchModel, CancellationToken cancellation);

        Task UpdatePasswordAsync(long userId, string hashedPassword, CancellationToken ct = default);
    }
}
