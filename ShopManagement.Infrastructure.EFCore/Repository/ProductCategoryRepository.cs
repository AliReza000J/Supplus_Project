using _0_Framework.Application;
using _0_Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Application.Contracts.ProductCategory;
using ShopManagement.Domain.ProductCategoryAgg;

namespace ShopManagement.Infrastructure.EFCore.Repository
{
    public class ProductCategoryRepository : BaseRepository<long, ProductCategory>, IProductCategoryRepository
    {
        private readonly ShopContext _context;

        public ProductCategoryRepository(ShopContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EditProductCategory?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _context.ProductCategories.Select(x => new EditProductCategory()
            {
                Id = x.Id,
                Description = x.Description,
                Name = x.Name,
                Keywords = x.Keywords,
                MetaDescription = x.MetaDescription,
                //Picture = x.Picture,
                PictureAlt = x.PictureAlt,
                PictureTitle = x.PictureTitle,
                Slug = x.Slug,
                ParentId = x.ParentId,
            }).FirstOrDefaultAsync(x => x.Id == id, cancellation);
        }

        public async Task<List<ProductCategoryViewModel>> GetLeafCategories(CancellationToken cancellation)
        {
            return await _context.ProductCategories
                .Where(x => !x.Children.Any())
                .Select(x => new ProductCategoryViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .AsNoTracking()
                .ToListAsync(cancellation);
        }

        public async Task<ProductCategory?> GetWithChildren(long id, CancellationToken cancellation)
        {
            return await _context.ProductCategories
                .Include(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id,
                cancellation);
        }
        public async Task<List<ProductCategoryViewModel>> GetProductCategories(CancellationToken cancellation)
        {
            return await _context.ProductCategories
                .Select(x => new ProductCategoryViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Parent = x.Parent.Name
                }).ToListAsync(cancellation);
        }

        public async Task<string?> GetFullSlugById(long id, CancellationToken cancellation)
        {
            return await _context.ProductCategories
                .Where(x => x.Id == id)
                .Select(x => x.FullSlug)
                .FirstOrDefaultAsync(cancellation);
        }

        public async Task<List<ProductCategoryViewModel>> Search(ProductCategorySearchModel searchModel, CancellationToken cancellation)
        {
            var query = _context.ProductCategories
                .Include(x => x.Parent)
                .Select(x => new ProductCategoryViewModel
                {
                    Id = x.Id,
                    Picture = x.Picture,
                    Name = x.Name,
                    CreationDate = x.CreationDate.ToFarsi(),
                    Parent = x.Parent != null ? x.Parent.Name : "—"
                });

            if (!string.IsNullOrWhiteSpace(searchModel.Name))
                query = query.Where(x => x.Name.Contains(searchModel.Name));

            return await query.OrderByDescending(x => x.Id).ToListAsync(cancellation);
        }
    }
}
