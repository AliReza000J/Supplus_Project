using _0_Framework.Application;
using _0_Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Application.Contracts.Product;
using ShopManagement.Domain.ProductAgg;

namespace ShopManagement.Infrastructure.EFCore.Repository
{
    public class ProductRepository: BaseRepository<long, Product>, IProductRepository
    {
        private readonly ShopContext _context;

        public ProductRepository(ShopContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EditProduct?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _context.Products.Select(x => new EditProduct
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Brand = x.Brand,
                Slug = x.Slug,
                CategoryId = x.CategoryId,
                Description = x.Description,
                Specifications = x.Specifications,
                Keywords = x.Keywords,
                MetaDescription = x.MetaDescription,
                PictureAlt = x.PictureAlt,
                PictureTitle = x.PictureTitle,
                ShortDescription = x.ShortDescription,
            }).FirstOrDefaultAsync(x => x.Id == id, cancellation);
        }

        public async Task<string?> GetProductName(long prodcutId, CancellationToken cancellation)
        {
            return await _context.Products
                .Where(x => x.Id == prodcutId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellation);
        }

        public async Task<string?> GetProductPic(long prodcutId, CancellationToken cancellation)
        {
            return await _context.Products
                .Where(x => x.Id == prodcutId)
                .Select(x => x.Picture)
                .FirstOrDefaultAsync(cancellation);
        }

        public async Task<List<ProductViewModel>> GetProducts(CancellationToken cancellation)
        {
            return await _context.Products.Select(x => new ProductViewModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync(cancellation);
        }

        public async Task<Product?> GetProductWithCategory(long id, CancellationToken cancellation)
        {
            return await _context.Products.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id, cancellation);
        }

        public async Task<List<ProductViewModel>> Search(ProductSearchModel searchModel, CancellationToken cancellation)
        {
            var query = _context.Products
                .Include(x => x.Category)
                .Select(x => new ProductViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Category = x.Category.Name,
                    CategoryId = x.CategoryId,
                    Code = x.Code,
                    Brand = x.Brand,
                    ViewCount = x.ViewCount,
                    Picture = x.Picture,
                    CreationDate = x.CreationDate.ToFarsi().ToPersianNumber()
                });

            if (!string.IsNullOrWhiteSpace(searchModel.Name))
                query = query.Where(x => x.Name.Contains(searchModel.Name));

            if (!string.IsNullOrWhiteSpace(searchModel.Code))
                query = query.Where(x => x.Code.Contains(searchModel.Code));

            if (!string.IsNullOrWhiteSpace(searchModel.Brand))
                query = query.Where(x => x.Brand.Contains(searchModel.Brand));

            if (searchModel.CategoryId != 0)
                query = query.Where(x => x.CategoryId == searchModel.CategoryId);

            return await query.OrderByDescending(x => x.Id).ToListAsync(cancellation);
        }
    }
}
