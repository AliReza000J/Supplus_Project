using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.DTOs;
using _01_PharmaPlusQuery.Contracts.Product;
using _01_PharmaPlusQuery.Contracts.ProductCategory;
using DiscountManagement.Infrastructure.EFCore;
using InventoryMangement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.ProductAgg;
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



        //public async Task<List<ProductCategoryQueryModel>> GetProductCategories()
        //{
        //    return await _shopContext.ProductCategories
        //        .Select(x => new ProductCategoryQueryModel
        //        {
        //            Id = x.Id,
        //            Name = x.Name,
        //            Picture = x.Picture,
        //            PictureAlt = x.PictureAlt,
        //            PictureTitle = x.PictureTitle,
        //            Slug = x.Slug,
        //            ProductsCount = x.Products.Count
        //        }).AsNoTracking().ToListAsync();
        //}

        public async Task<List<ProductCategoryQueryModel>> GetAll()
        {
            var categories = await _shopContext.ProductCategories
            .Select(x => new ProductCategoryQueryModel
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                FullSlug = x.FullSlug,
                ParentId = x.ParentId,
            })
            .AsNoTracking()
            .ToListAsync();

            var lookup = categories.ToLookup(c => c.ParentId);

            List<ProductCategoryQueryModel> BuildTree(long? parentId)
            {
                return lookup[parentId]
                    .Select(c => new ProductCategoryQueryModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Slug = c.Slug,
                        ParentId = c.ParentId,
                        Children = BuildTree(c.Id)
                    })
                    .ToList();
            }

            return BuildTree(null); // root-level categories
        }
        //public async Task<List<ProductCategoryQueryModel>> GetLeafProductCategories()
        //{
        //    return await _shopContext.ProductCategories
        //        .Where(x => !x.Children.Any())
        //        .Select(x => new ProductCategoryQueryModel
        //        {
        //            Id = x.Id,
        //            Name = x.Name,
        //            Picture = x.Picture,
        //            PictureAlt = x.PictureAlt,
        //            PictureTitle = x.PictureTitle,
        //            Slug = x.Slug,
        //            ProductsCount = x.Products.Count
        //        })
        //        .AsNoTracking()
        //        .ToListAsync();
        //}
        //public async Task<ProductCategoryQueryModel> GetLeafProductCategoryWithProductsBy(string slug) //leaves
        //{
        //    var inventory = await _inventoryContext.Inventory
        //        .Select(x => new { x.ProductId, x.UnitPrice })
        //        .AsNoTracking()
        //        .ToListAsync();

        //    var discounts = await _discountContext.CustomerDiscounts
        //        .Where(x => x.StartDate < DateTime.Now && x.EndDate > DateTime.Now)
        //        .Select(x => new { x.ProductId, x.DiscountRate, x.EndDate })
        //        .AsNoTracking()
        //        .ToListAsync();

        //    var category = await _shopContext.ProductCategories
        //        .Where(x => !x.Children.Any())
        //        .Include(x => x.Products)
        //        .ThenInclude(x => x.Category)
        //        .AsNoTracking()
        //        .Select(x => new ProductCategoryQueryModel
        //        {
        //            Id = x.Id,
        //            Name = x.Name,
        //            Description = x.Description,
        //            MetaDescription = x.MetaDescription,
        //            Keywords = x.Keywords,
        //            Slug = x.Slug,
        //            Picture = x.Picture,
        //            PictureAlt = x.PictureAlt,
        //            PictureTitle = x.PictureTitle,
        //            Products = x.Products.Select(p => new ProductQueryModel
        //            {
        //                Id = p.Id,
        //                Category = p.Category.Name,
        //                Name = p.Name,
        //                Picture = p.Picture,
        //                PictureAlt = p.PictureAlt,
        //                PictureTitle = p.PictureTitle,
        //                Slug = p.Slug,
        //            }).ToList(),
        //            ProductsCount = x.Products.Count,
        //        })
        //        .FirstOrDefaultAsync(x => x.Slug == slug);

        //    if (category == null)
        //        return null;

        //    foreach (var product in category.Products)
        //    {
        //        var productInventory = inventory.FirstOrDefault(x => x.ProductId == product.Id);
        //        if (productInventory == null)
        //            continue;

        //        var price = productInventory.UnitPrice;
        //        product.Price = price.ToMoney();

        //        var discount = discounts.FirstOrDefault(x => x.ProductId == product.Id);
        //        if (discount != null && discount.DiscountRate > 0)
        //        {
        //            var rate = discount.DiscountRate;
        //            product.DiscountRate = rate;
        //            product.DiscountExpireDate = discount.EndDate.ToDiscountFormat();
        //            product.HasDiscount = true;
        //            var discountAmount = Math.Round((price * rate) / 100);
        //            product.PriceWithDiscount = (price - discountAmount).ToMoney();
        //        }
        //    }

        //    return category;
        //}

        //public async Task<List<ProductCategoryQueryModel>> GetProductCategoriesWithProducts()
        //{
        //    var inventory = await _inventoryContext.Inventory
        //        .Select(x => new { x.ProductId, x.UnitPrice })
        //        .AsNoTracking()
        //        .ToListAsync();

        //    var discounts = await _discountContext.CustomerDiscounts
        //        .Where(x => x.StartDate < DateTime.Now && x.EndDate > DateTime.Now)
        //        .Select(x => new { x.ProductId, x.DiscountRate })
        //        .AsNoTracking()
        //        .ToListAsync();

        //    var categories = await _shopContext.ProductCategories
        //        .Include(x => x.Products)
        //        .ThenInclude(x => x.Category)
        //        .AsNoTracking()
        //        .Select(x => new ProductCategoryQueryModel
        //        {
        //            Id = x.Id,
        //            Name = x.Name,
        //            Slug = x.Slug,

        //            Products = x.Products.Select(p => new ProductQueryModel
        //            {
        //                Id = p.Id,
        //                Category = p.Category.Name,
        //                Name = p.Name,
        //                Picture = p.Picture,
        //                PictureAlt = p.PictureAlt,
        //                PictureTitle = p.PictureTitle,
        //                Slug = p.Slug
        //            }).ToList()
        //        })
        //        .ToListAsync();

        //    foreach (var category in categories)
        //    {
        //        foreach (var product in category.Products)
        //        {
        //            var inventoryRecord = inventory.FirstOrDefault(x => x.ProductId == product.Id);
        //            if (inventoryRecord == null)
        //                continue;

        //            var price = inventoryRecord.UnitPrice;
        //            product.Price = price.ToMoney();

        //            var discount = discounts.FirstOrDefault(x => x.ProductId == product.Id);
        //            if (discount != null && discount.DiscountRate > 0)
        //            {
        //                var rate = discount.DiscountRate;
        //                product.DiscountRate = rate;
        //                product.HasDiscount = true;
        //                var discountAmount = Math.Round((price * rate) / 100);
        //                product.PriceWithDiscount = (price - discountAmount).ToMoney();
        //            }
        //        }
        //    }

        //    return categories;
        //}

        public async Task<ProductCategoryQueryModel> GetCategoryByFullSlug(string fullSlug)
        {
            var now = DateTime.Now;

            // ===== Base Category =====
            var category = await _shopContext.ProductCategories
                .Include(x => x.Children)
                .Include(x => x.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FullSlug == fullSlug);

            if (category == null)
                return null;

            // ===== Load Inventory & Discounts =====
            var inventory = await _inventoryContext.Inventory
            .Select(x => new ProductInventoryDto { ProductId = x.ProductId, UnitPrice = x.UnitPrice })
            .ToListAsync();

            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => x.StartDate < now && x.EndDate > now)
                .Select(x => new ProductDiscountDto { ProductId = x.ProductId, DiscountRate = x.DiscountRate, EndDate = x.EndDate })
                .ToListAsync();

            var result = new ProductCategoryQueryModel
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                FullSlug = category.FullSlug,
                Description = category.Description,
                Picture = category.Picture,
                PictureAlt = category.PictureAlt,
                PictureTitle = category.PictureTitle,
                MetaDescription = category.MetaDescription,
                Keywords = category.Keywords,
                ParentId = category.ParentId,
                Children = new List<ProductCategoryQueryModel>(),
                Products = new List<ProductQueryModel>()
            };

            // ====== (Leaf Category) ======
            if (!category.Children.Any())
            {
                result.Products = category.Products
                    .Select(p => MapProductWithPrice(p, inventory, discounts))
                    .OrderByDescending(p => p.Id)
                    .ToList();

                result.ProductsCount = result.Products.Count;
            }
            else
            {
                foreach (var child in category.Children)
                {
                    var childProducts = await _shopContext.Products
                        .Where(p => p.CategoryId == child.Id)
                        .OrderByDescending(p => p.Id)
                        .Take(10)
                        .AsNoTracking()
                        .Select(p => MapProductWithPrice(p, inventory, discounts))
                        .ToListAsync();

                    result.Children.Add(new ProductCategoryQueryModel
                    {
                        Id = child.Id,
                        Name = child.Name,
                        Slug = child.Slug,
                        FullSlug = child.FullSlug,
                        Picture = child.Picture,
                        PictureAlt = child.PictureAlt,
                        PictureTitle = child.PictureTitle,
                        Products = childProducts,
                        ProductsCount = childProducts.Count()
                    });
                }

                result.ProductsCount = result.Children.Sum(c => c.ProductsCount);
            }

            return result;
        }

        private static ProductQueryModel MapProductWithPrice(Product p,
            List<ProductInventoryDto> inventory,
            List<ProductDiscountDto> discounts)
        {
            var product = new ProductQueryModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Picture = p.Picture,
                PictureAlt = p.PictureAlt,
                PictureTitle = p.PictureTitle,
                ShortDescription = p.ShortDescription
            };

            var inv = inventory.FirstOrDefault(x => x.ProductId == p.Id);
            if (inv != null)
            {
                var price = inv.UnitPrice;
                product.Price = price.ToMoney().ToPersianNumber();

                var discount = discounts.FirstOrDefault(x => x.ProductId == p.Id);
                if (discount != null && discount.DiscountRate > 0)
                {
                    var rate = discount.DiscountRate;
                    var discountAmount = Math.Round(price * rate / 100);
                    product.HasDiscount = true;
                    product.DiscountRate = rate;
                    product.DiscountExpireDate = discount.EndDate.ToFarsi().ToPersianNumber();
                    product.PriceWithDiscount = (price - discountAmount).ToMoney().ToPersianNumber();
                }
            }

            return product;
        }

    }
}
