using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Product;
using _01_PharmaPlusQuery.Contracts.ProductCategory;
using DiscountManagement.Infrastructure.EFCore;
using InventoryMangement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.EFCore;

namespace _01_PharmaPlusQuery.Query
{
    public class ProductCategoryQuery : IProductCategoryQuery
    {
        private readonly ShopContext _shopContext;
        private readonly InventoryContext _inventoryContext;
        private readonly DiscountContext _discountContext;

        public ProductCategoryQuery(ShopContext shopContext, InventoryContext inventoryContext, DiscountContext discountContext)
        {
            _shopContext = shopContext;
            _inventoryContext = inventoryContext;
            _discountContext = discountContext;
        }

        public async Task<List<ProductCategoryQueryModel>> GetProductCategories()
        {
            return await _shopContext.ProductCategories
                .Select(x => new ProductCategoryQueryModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Picture = x.Picture,
                    PictureAlt = x.PictureAlt,
                    PictureTitle = x.PictureTitle,
                    Slug = x.Slug
                }).AsNoTracking().ToListAsync();
        }

        public async Task<List<ProductCategoryQueryModel>> GetProductCategoriesWithProducts()
        {
            var inventory = await _inventoryContext.Inventory
                .Select(x => new { x.ProductId, x.UnitPrice })
                .AsNoTracking()
                .ToListAsync();

            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => x.StartDate < DateTime.Now && x.EndDate > DateTime.Now)
                .Select(x => new { x.ProductId, x.DiscountRate })
                .AsNoTracking()
                .ToListAsync();

            var categories = await _shopContext.ProductCategories
                .Include(x => x.Products)
                .ThenInclude(x => x.Category)
                .AsNoTracking()
                .Select(x => new ProductCategoryQueryModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    Products = x.Products.Select(p => new ProductQueryModel
                    {
                        Id = p.Id,
                        Category = p.Category.Name,
                        Name = p.Name,
                        Picture = p.Picture,
                        PictureAlt = p.PictureAlt,
                        PictureTitle = p.PictureTitle,
                        Slug = p.Slug
                    }).ToList()
                })
                .ToListAsync();

            foreach (var category in categories)
            {
                foreach (var product in category.Products)
                {
                    var inventoryRecord = inventory.FirstOrDefault(x => x.ProductId == product.Id);
                    if (inventoryRecord == null)
                        continue;

                    var price = inventoryRecord.UnitPrice;
                    product.Price = price.ToMoney();

                    var discount = discounts.FirstOrDefault(x => x.ProductId == product.Id);
                    if (discount != null && discount.DiscountRate > 0)
                    {
                        var rate = discount.DiscountRate;
                        product.DiscountRate = rate;
                        product.HasDiscount = true;
                        var discountAmount = Math.Round((price * rate) / 100);
                        product.PriceWithDiscount = (price - discountAmount).ToMoney();
                    }
                }
            }

            return categories;
        }

        public async Task<ProductCategoryQueryModel> GetProductCategoryWithProductsBy(string slug)
        {
            var inventory = await _inventoryContext.Inventory
                .Select(x => new { x.ProductId, x.UnitPrice })
                .AsNoTracking()
                .ToListAsync();

            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => x.StartDate < DateTime.Now && x.EndDate > DateTime.Now)
                .Select(x => new { x.ProductId, x.DiscountRate, x.EndDate })
                .AsNoTracking()
                .ToListAsync();

            var category = await _shopContext.ProductCategories
                .Include(x => x.Products)
                .ThenInclude(x => x.Category)
                .AsNoTracking()
                .Select(x => new ProductCategoryQueryModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    MetaDescription = x.MetaDescription,
                    Keywords = x.Keywords,
                    Slug = x.Slug,
                    Products = x.Products.Select(p => new ProductQueryModel
                    {
                        Id = p.Id,
                        Category = p.Category.Name,
                        Name = p.Name,
                        Picture = p.Picture,
                        PictureAlt = p.PictureAlt,
                        PictureTitle = p.PictureTitle,
                        Slug = p.Slug
                    }).ToList()
                })
                .FirstOrDefaultAsync(x => x.Slug == slug);

            if (category == null)
                return null;

            foreach (var product in category.Products)
            {
                var productInventory = inventory.FirstOrDefault(x => x.ProductId == product.Id);
                if (productInventory == null)
                    continue;

                var price = productInventory.UnitPrice;
                product.Price = price.ToMoney();

                var discount = discounts.FirstOrDefault(x => x.ProductId == product.Id);
                if (discount != null && discount.DiscountRate > 0)
                {
                    var rate = discount.DiscountRate;
                    product.DiscountRate = rate;
                    product.DiscountExpireDate = discount.EndDate.ToDiscountFormat();
                    product.HasDiscount = true;
                    var discountAmount = Math.Round((price * rate) / 100);
                    product.PriceWithDiscount = (price - discountAmount).ToMoney();
                }
            }

            return category;
        }
    }
}
