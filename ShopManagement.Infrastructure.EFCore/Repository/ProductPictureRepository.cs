using _0_Framework.Application;
using _0_Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Application.Contracts.ProductPicture;
using ShopManagement.Domain.ProductPictureAgg;

namespace ShopManagement.Infrastructure.EFCore.Repository
{
    public class ProductPictureRepository : BaseRepository<long, ProductPicture>, IProductPictureRepository
    {
        private readonly ShopContext _context;

        public ProductPictureRepository(ShopContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EditProductPicture?> GetDetails(long id)
        {
            return await _context.ProductPictures
                .Select(x => new EditProductPicture
                {
                    Id = x.Id,
                    PictureAlt = x.PictureAlt,
                    PictureTitle = x.PictureTitle,
                    ProductId = x.ProductId,
                }).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ProductPicture?> GetWithProductAndCategory(long id)
        {
            return await _context.ProductPictures
                .Include(x => x.Product)
                .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<ProductPictureViewModel>> Search(ProductPictureSearchModel searchModel)
        {
            var query = _context.ProductPictures
                .Include(x => x.Product)
                .Select(x => new ProductPictureViewModel
                {
                    Id = x.Id,
                    Product = x.Product.Name,
                    CreationDate = x.CreationDate.ToFarsi().ToPersianNumber(),
                    Picture = x.Picture,
                    ProductId = x.ProductId,
                    IsRemoved = x.IsRemoved
                });

            if (searchModel.ProductId != 0)
                query = query.Where(x => x.ProductId == searchModel.ProductId);

            return await query.OrderByDescending(x => x.Id).ToListAsync();
        }
    }
}
