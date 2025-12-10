using _0_Framework.Infrastructure;
using ShopManagement.Domain.UserProductInteractionAgg;

namespace ShopManagement.Infrastructure.EFCore.Repository
{
    public class ProductViewRepository: BaseRepository<long, ProductView>, IProductViewRepository
    {
        private readonly ShopContext _context;

        public ProductViewRepository(ShopContext context) : base(context)
        {
            _context = context;
        }
    }
}
