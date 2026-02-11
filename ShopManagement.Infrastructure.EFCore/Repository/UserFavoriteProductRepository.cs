using _0_Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.UserProductInteractionAgg;

namespace ShopManagement.Infrastructure.EFCore.Repository
{
    public class UserFavoriteProductRepository : BaseRepository<long, UserFavoriteProduct>, IUserFavoriteProductRepository
    {
        private readonly ShopContext _context;
        public UserFavoriteProductRepository(ShopContext context) : base(context)
        {
            _context = context;
        }

        public async Task<UserFavoriteProduct?> GetByUserIdAndProductId(long userId, long productId)
        {
            return await _context.UserFavoriteProducts
                .FirstOrDefaultAsync(x => x.UserId == userId &&
                x.ProductId == productId);
        }

        public void Remove(UserFavoriteProduct ufp)
        {
            _context.UserFavoriteProducts.Remove(ufp);
        }
    }
}
