using _0_Framework.Application;
using _01_LampshadeQuery.Contracts.Comment;
using _01_PharmaPlusQuery.Contracts.DTOs;
using _01_PharmaPlusQuery.Contracts.Product;
using CommentManagement.Domain.CommentAgg;
using CommnetManagement.Infrastructure.EFCore;
using DiscountManagement.Infrastructure.EFCore;
using InventoryMangement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.EFCore;
//using ShopManagement.Application.Contracts.Order;

namespace _01_PharmaPlusQuery.Query
{
    public class ProductQuery : IProductQuery
    {
        private readonly ShopContext _shopContext;
        private readonly InventoryContext _inventoryContext;
        private readonly DiscountContext _discountContext;
        private readonly CommentContext _commentContext;

        public ProductQuery(ShopContext context, InventoryContext inventoryContext,
        DiscountContext discountContext, CommentContext commentContext)
        {
            _shopContext = context;
            _discountContext = discountContext;
            _inventoryContext = inventoryContext;
            _commentContext = commentContext;
        }

        public async Task<ProductQueryModel> GetProductDetails(string productSlug)
        {
            var product = await _shopContext.Products
            .Include(x => x.Category)
            .Include(x => x.ProductPictures)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == productSlug);

            if (product == null)
                return null;

            var inventoryTask = _inventoryContext.Inventory
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == product.Id);

            var discountTask = _discountContext.CustomerDiscounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.ProductId == product.Id &&
                x.StartDate <= DateTime.Now &&
                x.EndDate >= DateTime.Now);

            await Task.WhenAll(inventoryTask, discountTask);

            var inventory = inventoryTask.Result;
            var discount = discountTask.Result;

            double unitPrice = inventory?.UnitPrice ?? 0;
            bool hasDiscount = discount != null && discount.DiscountRate > 0;

            double priceWithDiscount = unitPrice;
            if (hasDiscount)
                priceWithDiscount = Math.Round(unitPrice * (100 - discount.DiscountRate) / 100);

            var result = new ProductQueryModel
            {
                Id = product.Id,
                Name = product.Name,
                Code = product.Code,
                Description = product.Description,
                ShortDescription = product.ShortDescription,
                MetaDescription = product.MetaDescription,
                Specifications = product.Specifications,
                Keywords = product.Keywords,
                Slug = product.Slug,
                Category = product.Category?.Name,
                CategorySlug = product.Category?.Slug,
                CategoryFullSlug = product.Category?.FullSlug,
                Picture = product.Picture,
                PictureAlt = product.PictureAlt,
                PictureTitle = product.PictureTitle,
                Pictures = product.ProductPictures
                .Where(x => !x.IsRemoved)
                .Select(x => new ProductPictureQueryModel
                {
                    Picture = x.Picture,
                    PictureAlt = x.PictureAlt,
                    PictureTitle = x.PictureTitle,
                    ProductId = product.Id,
                }).ToList(),
                    Price = unitPrice.ToMoney(),
                    HasDiscount = hasDiscount,
                    DiscountRate = hasDiscount ? discount.DiscountRate : 0,
                    DiscountExpireDate = hasDiscount ? discount.EndDate.ToDiscountFormat() : "",
                    PriceWithDiscount = hasDiscount ? priceWithDiscount.ToMoney() : unitPrice.ToMoney()
                };

            result.Comments = await _commentContext.Comments
                .Where(x => !x.IsCanceled)
                .Where(x => x.IsConfirmed)
                .Where(x => x.Type == ((int)CommentType.Product))
                .Where(x => x.OwnerRecordId == product.Id)
                .Select(x => new CommentQueryModel
                {
                    Id = x.Id,
                    Message = x.Message,
                    Username = x.Username,
                    CreationDate = x.CreationDate.ToFarsi(),
                    Reply = x.AdminReply,
                    DislikeCount = x.DislikeCount,
                    LikeCount = x.LikeCount,
                    Rate = x.Rate,
                    ReplyDate = x.AdminReplyDate.ToFarsi(),
                    IsBuyer = x.IsBuyer
                }).OrderByDescending(x => x.Id).ToListAsync();

            if(result.Comments.Any())
            {
                int sum = 0;
                foreach (var comment in result.Comments.Where(comment => comment.Rate >= 1 && comment.Rate <= 5))
                {
                    sum += comment.Rate;
                }

                int count = result.Comments.Count;

                result.AvgRate = sum / count;
            }
            else
            {
                result.AvgRate = 0;
            }

                return result;
        }

        //private static List<ProductPictureQueryModel> MapProductPictures(List<ProductPicture> pictures)
        //{
        //    return pictures.Select(x => new ProductPictureQueryModel
        //    {
        //        IsRemoved = x.IsRemoved,
        //        Picture = x.Picture,
        //        PictureAlt = x.PictureAlt,
        //        PictureTitle = x.PictureTitle,
        //        ProductId = x.ProductId
        //    }).Where(x => !x.IsRemoved).ToList();
        //}

        public async Task<List<ProductQueryModel>> GetLatestArrivals()
        {
            var now = DateTime.Now;

            // ===== Load Inventory & Discounts =====
            var inventory = await _inventoryContext.Inventory
            .Select(x => new ProductInventoryDto { ProductId = x.ProductId, UnitPrice = x.UnitPrice })
            .ToListAsync();

            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => x.StartDate < now && x.EndDate > now)
                .Select(x => new ProductDiscountDto { ProductId = x.ProductId, DiscountRate = x.DiscountRate, EndDate = x.EndDate })
                .ToListAsync();

            var products = await _shopContext.Products.Include(x => x.Category)
                .Select(product => new ProductQueryModel
                {
                    Id = product.Id,
                    Category = product.Category.Name,
                    Name = product.Name,
                    Picture = product.Picture,
                    PictureAlt = product.PictureAlt,
                    PictureTitle = product.PictureTitle,
                    Slug = product.Slug,
                    MetaDescription = product.MetaDescription
                }).AsNoTracking().OrderByDescending(x => x.Id).Take(10).ToListAsync();

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
                CategoryFullSlug = product.Category.FullSlug,
                Name = product.Name,
                Picture = product.Picture,
                PictureAlt = product.PictureAlt,
                PictureTitle = product.PictureTitle,
                ShortDescription = product.ShortDescription,
                Slug = product.Slug,
                Keywords = product.Keywords,
                MetaDescription = product.MetaDescription,
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