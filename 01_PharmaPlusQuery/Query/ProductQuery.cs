using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Product;
//using CommnetManagement.Infrastructure.EFCore;
using DiscountManagement.Infrastructure.EFCore;
using InventoryMangement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.ProductPictureAgg;
using ShopManagement.Infrastructure.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
//using ShopManagement.Application.Contracts.Order;

namespace _01_PharmaPlusQuery.Query
{
    public class ProductQuery : IProductQuery
    {
        private readonly ShopContext _shopContext;
        private readonly InventoryContext _inventoryContext;
        private readonly DiscountContext _discountContext;
        //private readonly CommentContext _commentContext;

        public ProductQuery(ShopContext context, InventoryContext inventoryContext,
            DiscountContext discountContext)
            //DiscountContext discountContext, CommentContext commentContext)
        {
            _shopContext = context;
            _discountContext = discountContext;
            _inventoryContext = inventoryContext;
            //_commentContext = commentContext;
        }

        public async Task<ProductQueryModel> GetProductDetails(string slug)
        {
            var inventory = await _inventoryContext.Inventory.Select
                (x => new {x.ProductId, x.UnitPrice, x.InStock}).ToListAsync();

            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => x.StartDate < DateTime.Now && x.EndDate > DateTime.Now)
                .Select(x => new {x.DiscountRate, x.ProductId, x.EndDate}).ToListAsync();

            var product = await _shopContext.Products
                .Include(x => x.Category)
                .Include(x => x.ProductPictures)
                .Select(x => new ProductQueryModel
                {
                    Id = x.Id,
                    Category = x.Category.Name,
                    Name = x.Name,
                    Picture = x.Picture,
                    PictureAlt = x.PictureAlt,
                    PictureTitle = x.PictureTitle,
                    Slug = x.Slug,
                    CategorySlug = x.Category.Slug,
                    Code = x.Code,
                    Description = x.Description,
                    Keywords = x.Keywords,
                    MetaDescription = x.MetaDescription,
                    ShortDescription = x.ShortDescription,
                    Pictures = MapProductPictures(x.ProductPictures)
                }).AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug);

            if (product == null)
                return new ProductQueryModel();

            var productInventory = inventory.FirstOrDefault(x => x.ProductId == product.Id);
            if (productInventory != null)
            {
                product.IsInStock = productInventory.InStock;
                var price = productInventory.UnitPrice;
                product.Price = price.ToMoney();
                product.DoublePrice = price;
                var discount = discounts.FirstOrDefault(x => x.ProductId == product.Id);
                if (discount != null)
                {
                    var discountRate = discount.DiscountRate;
                    product.DiscountRate = discountRate;
                    product.DiscountExpireDate = discount.EndDate.ToDiscountFormat();
                    product.HasDiscount = discountRate > 0;
                    var discountAmount = Math.Round((price * discountRate) / 100);
                    product.PriceWithDiscount = (price - discountAmount).ToMoney();
                }
            }

            //product.Comments = _commentContext.Comments
            //    .Where(x => !x.IsCanceled)
            //    .Where(x => x.IsConfirmed)
            //    .Where(x => x.Type == CommentType.Product)
            //    .Where(x => x.OwnerRecordId == product.Id)
            //    .Select(x => new CommentQueryModel
            //    {
            //        Id = x.Id,
            //        Message = x.Message,
            //        Name = x.Name,
            //        CreationDate = x.CreationDate.ToFarsi()
            //    }).OrderByDescending(x => x.Id).ToList();

            return product;
        }

        private static List<ProductPictureQueryModel> MapProductPictures(List<ProductPicture> pictures)
        {
            return pictures.Select(x => new ProductPictureQueryModel
            {
                IsRemoved = x.IsRemoved,
                Picture = x.Picture,
                PictureAlt = x.PictureAlt,
                PictureTitle = x.PictureTitle,
                ProductId = x.ProductId
            }).Where(x => !x.IsRemoved).ToList();
        }

        public async Task<List<ProductQueryModel>> GetLatestArrivals()
        {
            var inventory = await _inventoryContext.Inventory
                .Select(x => new {x.ProductId, x.UnitPrice}).ToListAsync();
            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => x.StartDate < DateTime.Now && x.EndDate > DateTime.Now)
                .Select(x => new {x.DiscountRate, x.ProductId}).ToListAsync();
            var products = await _shopContext.Products.Include(x => x.Category)
                .Select(product => new ProductQueryModel
                {
                    Id = product.Id,
                    Category = product.Category.Name,
                    Name = product.Name,
                    Picture = product.Picture,
                    PictureAlt = product.PictureAlt,
                    PictureTitle = product.PictureTitle,
                    Slug = product.Slug
                }).AsNoTracking().OrderByDescending(x => x.Id).Take(6).ToListAsync();

            foreach (var product in products)
            {
                var productInventory = inventory.FirstOrDefault(x => x.ProductId == product.Id);
                if (productInventory != null)
                {
                    var price = productInventory.UnitPrice;
                    product.Price = price.ToMoney();
                    var discount = discounts.FirstOrDefault(x => x.ProductId == product.Id);
                    if (discount != null)
                    {
                        int discountRate = discount.DiscountRate;
                        product.DiscountRate = discountRate;
                        product.HasDiscount = discountRate > 0;
                        var discountAmount = Math.Round((price * discountRate) / 100);
                        product.PriceWithDiscount = (price - discountAmount).ToMoney();
                    }
                }
            }

            return products;
        }

        public async Task<List<ProductQueryModel>> Search(string value)
        {
            var query = _shopContext.Products
            .Include(x => x.Category)
            .Select(product => new ProductQueryModel
            {
                Id = product.Id,
                Category = product.Category.Name,
                CategorySlug = product.Category.Slug,
                Name = product.Name,
                Picture = product.Picture,
                PictureAlt = product.PictureAlt,
                PictureTitle = product.PictureTitle,
                ShortDescription = product.ShortDescription,
                Slug = product.Slug,
                Keywords = product.Keywords
            })
            .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value.Trim().ToLower()
                             .Replace("ي", "ی")
                             .Replace("ك", "ک");

                query = query.Where(x =>
                    EF.Functions.Like(x.Name.ToLower(), $"%{value}%") ||
                    EF.Functions.Like(x.ShortDescription.ToLower(), $"%{value}%") ||
                    EF.Functions.Like(x.Keywords.ToLower(), $"%{value}%") ||
                    EF.Functions.Like(x.Category.ToLower(), $"%{value}%"));
            }

            var products = await query.OrderByDescending(x => x.Id).ToListAsync();
            if (products.Count == 0)
                return products;

            var productIds = products.Select(x => x.Id).ToList();

            var inventory = await _inventoryContext.Inventory
                .Where(x => productIds.Contains(x.ProductId))
                .Select(x => new { x.ProductId, x.UnitPrice })
                .ToDictionaryAsync(x => x.ProductId, x => x.UnitPrice);

            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => productIds.Contains(x.ProductId)
                            && x.StartDate < DateTime.Now
                            && x.EndDate > DateTime.Now)
                .Select(x => new { x.ProductId, x.DiscountRate, x.EndDate })
                .ToDictionaryAsync(x => x.ProductId, x => new { x.DiscountRate, x.EndDate });

            foreach (var product in products)
            {
                if (inventory.TryGetValue(product.Id, out var price))
                {
                    product.Price = price.ToMoney();

                    if (discounts.TryGetValue(product.Id, out var discount) && discount.DiscountRate > 0)
                    {
                        int rate = discount.DiscountRate;
                        product.DiscountRate = rate;
                        product.DiscountExpireDate = discount.EndDate.ToDiscountFormat();
                        product.HasDiscount = true;

                        var discountAmount = Math.Round((price * rate) / 100);
                        product.PriceWithDiscount = (price - discountAmount).ToMoney();
                    }
                }
            }

            return products;
        }


        //public List<CartItem> CheckInventoryStatus(List<CartItem> cartItems)
        //{
        //    var inventory = _inventoryContext.Inventory.ToList();

        //    foreach (var cartItem in cartItems.Where(cartItem =>
        //        inventory.Any(x => x.ProductId == cartItem.Id && x.InStock)))
        //    {
        //        var itemInventory = inventory.Find(x => x.ProductId == cartItem.Id);
        //        cartItem.IsInStock = itemInventory.CalculateCurrentCount() >= cartItem.Count;
        //    }

        //    return cartItems;
        //}
    }
}