using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.DTOs;
using _01_PharmaPlusQuery.Contracts.Product;
using _01_PharmaPlusQuery.Contracts.ProductCategory;
using DiscountManagement.Domain.DiscountAgg;
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

        public async Task<List<ProductCategoryQueryModel>> GetAll(CancellationToken cancellation)
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
            .ToListAsync(cancellation);

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

        public async Task<ProductCategoryQueryModel?> GetCategoryByFullSlug(string fullSlug, CancellationToken cancellation)
        {
            var now = DateTime.Now;

            // ===== Base Category =====
            var category = await _shopContext.ProductCategories
                .Include(x => x.Children)
                .Include(x => x.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FullSlug == fullSlug, cancellation);

            if (category == null)
                return null;

            //// ===== Load Inventory & Discounts =====
            //var inventory = await _inventoryContext.Inventory
            //.Select(x => new ProductInventoryDto { ProductId = x.ProductId, UnitPrice = x.UnitPrice })
            //.ToListAsync(cancellation);

            //var activeDiscounts = await _discountContext.Discounts
            //.Include(x => x.Targets)
            //.Where(x => x.IsActive && x.Type == (Discount.DiscountType)1
            //&& x.ApplyType == (Discount.DiscountApplyType)1) // 1: Automatic, 1: OnProduct
            //.Where(x => x.StartDate < now && x.EndDate > now)
            //.ToListAsync(cancellation);

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
                Children = category.Children.Select(
                    child => new ProductCategoryQueryModel
                    {
                        Id = child.Id,
                        Name = child.Name,
                        Slug = child.Slug,
                        FullSlug = child.FullSlug,
                        Picture = child.Picture,
                        PictureAlt = child.PictureAlt,
                        PictureTitle = child.PictureTitle
                    }).ToList(),
            };

            //// ====== (Leaf Category) ======
            //if (!category.Children.Any())
            //{
            //    result.Products = category.Products
            //        .Select(p => MapProductWithPrice(p, inventory, activeDiscounts))
            //        .OrderByDescending(p => p.Id)
            //        .ToList();

            //    result.ProductsCount = result.Products.Count;
            //}
            //else
            //{
            //    foreach (var child in category.Children)
            //    {
            //        var childProductsRaw = await _shopContext.Products
            //            .Where(p => p.CategoryId == child.Id)
            //            .OrderByDescending(p => p.Id)
            //            .Take(10)
            //            .AsNoTracking()
            //            .ToListAsync(cancellation);

            //        var childProducts = childProductsRaw
            //            .Select(p => MapProductWithPrice(p, inventory, activeDiscounts))
            //            .ToList();

            //        result.Children.Add(new ProductCategoryQueryModel
            //        {

            //            Products = childProducts,
            //            ProductsCount = childProducts.Count()
            //        });
            //    }

            //    result.ProductsCount = result.Children.Sum(c => c.ProductsCount);
            //}

            return result;
        }

        //private static ProductQueryModel MapProductWithPrice(Product p,
        //    List<ProductInventoryDto> inventory,
        //    List<Discount> activeDiscounts)
        //{
        //    var product = new ProductQueryModel
        //    {
        //        Id = p.Id,
        //        CategoryId = p.CategoryId,
        //        Brand = p.Brand,
        //        Name = p.Name,
        //        Slug = p.Slug,
        //        Picture = p.Picture,
        //        PictureAlt = p.PictureAlt,
        //        PictureTitle = p.PictureTitle,
        //        ShortDescription = p.ShortDescription
        //    };

        //    var inv = inventory.FirstOrDefault(x => x.ProductId == p.Id);
        //    if (inv != null)
        //    {
        //        var price = inv.UnitPrice;
        //        product.Price = price.ToMoney();
        //        product.IsInStock = inv.InStock;

        //        var bestDiscount = activeDiscounts
        //            .Where(d => d.Targets.Any(t =>
        //                t.TargetType == (DiscountTarget.DiscountTargetType)4 || // اعمال به همه
        //                (t.TargetType == (DiscountTarget.DiscountTargetType)1 && t.ProductId == p.Id) || // تخفیف محصولی
        //                (t.TargetType == (DiscountTarget.DiscountTargetType)3 && t.CategoryId == p.CategoryId) || // تخفیف دسته‌بندی
        //                (t.TargetType == (DiscountTarget.DiscountTargetType)2 && !string.IsNullOrEmpty(p.Brand) && t.BrandName?.ToLower() == p.Brand.ToLower()) // تخفیف برندی
        //            ))
        //            .OrderByDescending(x => x.Percentage)
        //            .FirstOrDefault();

        //        if (bestDiscount != null)
        //        {
        //            var rate = bestDiscount.Percentage;
        //            var discountAmount = Math.Round(price * rate / 100);
        //            product.HasDiscount = true;
        //            product.DiscountRate = rate;
        //            product.DiscountExpireDate = bestDiscount.EndDate?.ToFarsi() ?? "";
        //            product.PriceWithDiscount = (price - discountAmount).ToMoney();
        //        }
        //        else
        //        {
        //            product.PriceWithDiscount = product.Price;
        //            product.HasDiscount = false;
        //        }
        //    }

        //    return product;
        //}

    }
}
