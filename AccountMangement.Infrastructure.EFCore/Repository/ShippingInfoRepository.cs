using _0_Framework.Infrastructure;
using AccountManagement.Domain.ShippingInfoAgg;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.Infrastructure.EFCore.Repository
{
    public class ShippingInfoRepository : RepositoryBase<long, ShippingInfo>, IShippingInfoRepository
    {
        private readonly AccountContext _context;
        public ShippingInfoRepository(AccountContext context) : base(context)
        {
            _context = context;
        }

        public void Delete(long id)
        {
            var entity = new ShippingInfo { Id = id };
            _context.ShippingInfos.Remove(entity);
        }

        public async Task<List<ShippingInfo>> GetAllByUserIdAsync(long userId, CancellationToken cancellation)
        {
            return await _context.ShippingInfos
           .Where(x => x.UserId == userId)
           .OrderByDescending(x => x.Id)
           .ToListAsync(cancellationToken: cancellation);
        }
    }
}
