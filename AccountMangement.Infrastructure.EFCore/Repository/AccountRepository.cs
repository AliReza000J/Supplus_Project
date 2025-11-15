using _0_Framework.Application;
using _0_Framework.Infrastructure;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Domain.AccountAgg;
using Microsoft.EntityFrameworkCore;


namespace AccountManagement.Infrastructure.EFCore.Repository
{
    public class AccountRepository : BaseRepository<long, Account>, IAccountRepository
    {
        private readonly AccountContext _context;

        public AccountRepository(AccountContext context) : base(context)
        {
            _context = context;
        }

        public EditAccount GetDetails(long id)
        {
            return _context.Accounts.Select(x => new EditAccount
            {
                Id = x.Id,
                Fullname = x.Fullname,
                Mobile = x.Mobile,
                RoleId = x.RoleId,
                Username = x.Username,
                Email = x.Email
            }).FirstOrDefault(x => x.Id == id);
        }

        public List<AccountViewModel> GetAccounts()
        {
            return _context.Accounts.Select(x => new AccountViewModel
            {
                Id = x.Id,
                Fullname = x.Fullname,
                RoleName = x.Role.Name,
                Username = x.Username,
                IsActive = x.IsActive
            }).ToList();
        }

        public List<AccountViewModel> Search(AccountSearchModel searchModel)
        {
            var query = _context.Accounts.Include(x => x.Role).Select(x => new AccountViewModel
            {
                Id = x.Id,
                Fullname = x.Fullname,
                Mobile = x.Mobile,
                ProfilePhoto = x.ProfilePhoto,
                RoleName = x.Role.Name,
                RoleId = x.RoleId,
                Username = x.Username,
                IsActive = x.IsActive,
                CreationDate = x.CreationDate.ToFarsi()
            });

            if (!string.IsNullOrWhiteSpace(searchModel.Username))
                query = query.Where(x => x.Username.Contains(searchModel.Username));

            if (!string.IsNullOrWhiteSpace(searchModel.Mobile))
                query = query.Where(x => x.Mobile.Contains(searchModel.Mobile));

            if (searchModel.RoleId > 0)
                query = query.Where(x => x.RoleId == searchModel.RoleId);

            if (searchModel.IsActive != null)
                query = query.Where(x => x.IsActive == searchModel.IsActive);

            return query.OrderByDescending(x => x.Id).ToList();
        }

        public bool Exists(string username, string mobile)
        {
            return _context.Accounts.Any(x => x.Username == username || x.Mobile == mobile);
        }

        public Account GetByUsername(string username)
        {
            return _context.Accounts.FirstOrDefault(x => x.Username == username);
        }
    }
}