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
using System.Text.Json;
using static DiscountManagement.Domain.DiscountAgg.Discount;
using static DiscountManagement.Domain.DiscountAgg.DiscountTarget;

namespace _01_PharmaPlusQuery.Query
{
    public class ProductQuery : IProductQuery
    {
        private readonly ShopContext _shopContext;
        private readonly InventoryContext _inventoryContext;
        private readonly DiscountContext _discountContext;
        private readonly CommentContext _commentContext;
        private readonly ICacheService _cacheService;

        public ProductQuery(ShopContext context, InventoryContext inventoryContext,
        DiscountContext discountContext, CommentContext commentContext, ICacheService cache)
        {
            _shopContext = context;
            _discountContext = discountContext;
            _inventoryContext = inventoryContext;
            _commentContext = commentContext;
            _cacheService = cache;
        }

        public async Task<ProductQueryModel?> GetProductDetails(string productSlug, long userId, CancellationToken cancellationToken = default)
        {
            var productBaseCacheKey = $"product:base:{productSlug}";
            var now = DateTime.Now;

            var productBase = await _cacheService.GetOrCreateAsync(
                productBaseCacheKey,
                factory: async () =>
                {
                    return await _shopContext.Products
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(x => x.Slug == productSlug)
                    .Select(x => new
                    {
                        x.Id,
                        x.CategoryId,
                        x.Name,
                        x.Code,
                        x.Brand,
                        x.Description,
                        x.ShortDescription,
                        x.MetaDescription,
                        x.Specifications,
                        x.Keywords,
                        x.Slug,
                        x.ViewCount,
                        x.Picture,
                        x.PictureAlt,
                        x.PictureTitle,
                        CategoryName = x.Category.Name,
                        CategorySlug = x.Category.Slug,
                        CategoryFullSlug = x.Category.FullSlug
                    })
                    .FirstOrDefaultAsync(cancellationToken);
                },
                absoluteExpiration: TimeSpan.FromHours(1),
                slidingExpiration: TimeSpan.FromMinutes(20),
                size: 1,
                cancellationToken: cancellationToken);

            if (productBase == null)
                return null;

            long productId = productBase.Id;
            var cutoffTime = now.AddDays(-7);
            var picturesCacheKey = $"product:pictures:{productBase.Id}";

            var pictures = await _cacheService.GetOrCreateAsync(
                key: picturesCacheKey,
                factory: async () =>
                {
                    return await _shopContext.ProductPictures
                        .AsNoTracking()
                        .Where(p => p.ProductId == productBase.Id && !p.IsRemoved)
                        .Select(p => new ProductPictureQueryModel
                        {
                            Picture = p.Picture,
                            PictureAlt = p.PictureAlt,
                            PictureTitle = p.PictureTitle,
                            ProductId = productBase.Id
                        })
                        .ToListAsync(cancellationToken);
                },
                absoluteExpiration: TimeSpan.FromHours(3),
                slidingExpiration: TimeSpan.FromMinutes(30),
                size: 5,
                cancellationToken: cancellationToken);

            var recentViews = await _shopContext.ProductViews
                .AsNoTracking()
                .CountAsync(v => v.ProductId == productId && v.CreationDate >= cutoffTime, cancellationToken);

            var totalFaved = await _shopContext.UserFavoriteProducts
                .AsNoTracking()
                .CountAsync(v => v.ProductId == productId, cancellationToken);

            var inCartCount = await _shopContext.CartItems
                .AsNoTracking()
                .CountAsync(x => x.ProductId == productId, cancellationToken);

            var currentUserfaved = await _shopContext.UserFavoriteProducts
                .AnyAsync(x => x.ProductId == productId && x.UserId == userId, cancellationToken);

            var inventoryTask = _inventoryContext.Inventory
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);

            var discountTask = _discountContext.Discounts
                .Include(x => x.Targets)
                .Where(x => x.IsActive && x.Type == (DiscountType)1 && x.ApplyType == (DiscountApplyType)1)
                .Where(x => x.StartDate < now && x.EndDate > now)
                .Where(x => x.Targets.Any(t =>
                    t.TargetType == (DiscountTargetType)4 || // برای همه
                    (t.TargetType == (DiscountTargetType)1 && t.ProductId == productId) || // برای این محصول
                    (t.TargetType == (DiscountTargetType)3 && t.CategoryId == productBase.CategoryId) || // برای این دسته
                    (t.TargetType == (DiscountTargetType)2 && t.BrandName == productBase.Brand) // برای این برند
                ))
                .OrderByDescending(x => x.Percentage)
                .FirstOrDefaultAsync(cancellationToken);

            var commentsRawTask = _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.RecordId == productId)
                .AsNoTracking()
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
                    IsBuyer = x.IsBuyer,
                    HasEdited = x.CreationDate != x.LastModified,
                    UserId = x.UserId,
                    LastModifedDate = x.LastModified.ToFarsi(),
                    LikedByUserIds = new List<long>(),
                    DislikedByUserIds = new List<long>()
                })
                .OrderByDescending(x => x.Id)
                .ToListAsync(cancellationToken);

            await Task.WhenAll(inventoryTask, discountTask, commentsRawTask);

            var inventory = inventoryTask.Result;
            var discount = discountTask.Result;
            var commentsRaw = commentsRawTask.Result;

            var commentIds = commentsRaw.Select(c => c.Id).ToList();

            var reactions = await _commentContext.CommentReactions
                .Where(r => commentIds.Contains(r.CommentId))
                .AsNoTracking()
                .Select(r => new
                {
                    r.CommentId,
                    r.Reaction,
                    r.UserId
                })
                .ToListAsync(cancellationToken);

            var reactionsLookup = reactions
                .GroupBy(r => r.CommentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var comment in commentsRaw)
            {
                if (!reactionsLookup.TryGetValue(comment.Id, out var relatedReactions))
                    continue;

                comment.LikedByUserIds = relatedReactions
                    .Where(r => r.Reaction == ReactionType.Like)
                    .Select(r => r.UserId)
                    .Where(id => id > 0)
                    .ToList();

                comment.DislikedByUserIds = relatedReactions
                    .Where(r => r.Reaction == ReactionType.Dislike)
                    .Select(r => r.UserId)
                    .Where(id => id > 0)
                    .ToList();
            }

            double unitPrice = inventory?.UnitPrice ?? 0;
            bool hasDiscount = discount != null;
            int discountPercentage = discount?.Percentage ?? 0;
            int discountedQty = hasDiscount ? (inventory?.InStock == true ? 99 : 0) : 0;
            double priceWithDiscount = hasDiscount
                ? Math.Round(unitPrice * (100 - discountPercentage) / 100)
                : unitPrice;

            int rateCount = commentsRaw.Count(c => c.Rate >= 1 && c.Rate <= 5);
            int commentCount = commentsRaw.Count(c => !string.IsNullOrWhiteSpace(c.Message));
            float avgRate = rateCount == 0 ? 0 : (float)commentsRaw.Where(c => c.Rate >= 1 && c.Rate <= 5).Average(c => c.Rate);

            var comments = commentsRaw.Where(c => !string.IsNullOrWhiteSpace(c.Message)).ToList();

            return new ProductQueryModel
            {
                Id = productBase.Id,
                Name = productBase.Name,
                Code = productBase.Code,
                Brand = productBase.Brand,
                Description = productBase.Description,
                ShortDescription = productBase.ShortDescription,
                MetaDescription = productBase.MetaDescription,
                Specifications = !string.IsNullOrWhiteSpace(productBase.Specifications)
                ? JsonSerializer.Deserialize<ProductSpecificationModel>(productBase.Specifications, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                : new ProductSpecificationModel(),
                Keywords = productBase.Keywords,
                Slug = productBase.Slug,
                ViewCount = productBase.ViewCount,
                IsCurrentUserFaved = currentUserfaved,

                CategoryName = productBase.CategoryName,
                CategorySlug = productBase.CategorySlug,
                CategoryFullSlug = productBase.CategoryFullSlug,

                Picture = productBase.Picture,
                PictureAlt = productBase.PictureAlt,
                PictureTitle = productBase.PictureTitle,

                Pictures = pictures,

                Price = unitPrice.ToMoney(),
                DoublePrice = unitPrice,
                HasDiscount = hasDiscount,
                DiscountRate = discountPercentage,
                DiscountExpireDate = discount?.EndDate?.ToFarsi() ?? "",
                PriceWithDiscount = priceWithDiscount.ToMoney(),
                DiscountedQty = discountedQty,
                IsInStock = inventory != null && inventory.InStock,
                InStockQty = inventory?.CalculateCurrentCount() ?? 0,

                Comments = comments,
                CommentCount = commentCount,
                RateCount = rateCount,
                AvgRate = avgRate,
                InFavedCount = totalFaved,
                InCartCount = inCartCount
            };
        }

        public async Task<List<ProductCardModel>> GetLatestArrivals(CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;

            // ===== Load Inventory & Discounts & Rates =====
            var inventory = await _inventoryContext.Inventory
            .Select(x => new ProductInventoryDto
            { ProductId = x.ProductId, UnitPrice = x.UnitPrice, InStock = x.InStock })
            .ToListAsync(cancellationToken);

            var activeDiscounts = await _discountContext.Discounts
                .Include(x => x.Targets)
                .Where(x => x.IsActive && (int)x.Type == 1) // 1 = Automatic
                .Where(x => (int)x.ApplyType == 1)          // 1 = OnProduct
                .Where(x => x.StartDate < now && x.EndDate > now)
                .ToListAsync(cancellationToken);

            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.RecordId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync(cancellationToken);

            var products = await _shopContext.Products.Include(x => x.Category)
                .Select(product => new ProductCardModel
                {
                    Id = product.Id,
                    CategoryId = product.CategoryId,
                    Brand = product.Brand,
                    CategoryName = product.Category.Name,
                    CategoryFullSlug = product.Category.FullSlug,
                    Name = product.Name,
                    Picture = product.Picture,
                    PictureAlt = product.PictureAlt,
                    PictureTitle = product.PictureTitle,
                    Slug = product.Slug,
                }).AsNoTracking().OrderByDescending(x => x.Id).Take(10).ToListAsync(cancellationToken);

            foreach (var product in products)
            {
                var productInventory = inventory.FirstOrDefault(x => x.ProductId == product.Id);
                if (productInventory != null)
                {
                    var price = productInventory.UnitPrice;
                    product.Price = price.ToMoney();
                    product.IsInStock = productInventory.InStock;

                    var bestDiscount = activeDiscounts
                        .Where(d => d.Targets.Any(t =>
                            t.TargetType == (DiscountTargetType)4 || // برای همه
                            (t.TargetType == (DiscountTargetType)1 && t.ProductId == product.Id) || // تطبیق محصول
                            (t.TargetType == (DiscountTargetType)3 && t.CategoryId == product.CategoryId) || // تطبیق دسته بندی
                            (t.TargetType == (DiscountTargetType)2 && t.BrandName == product.Brand) // تطبیق برند
                        ))
                        .OrderByDescending(x => x.Percentage)
                        .FirstOrDefault();

                    if (bestDiscount != null)
                    {
                        product.DiscountRate = bestDiscount.Percentage;
                        product.HasDiscount = true;
                        product.DiscountExpireDate = bestDiscount.EndDate?.ToString();

                        var discountAmount = Math.Round((price * bestDiscount.Percentage) / 100);
                        product.PriceWithDiscount = (price - discountAmount).ToMoney();
                    }
                    else
                    {
                        product.PriceWithDiscount = product.Price;
                        product.HasDiscount = false;
                    }
                }
                var rateInfo = avgRates.FirstOrDefault(x => x.ProductId == product.Id);
                if (rateInfo != null)
                {
                    product.AvgRate = (float)rateInfo.AvgRate;
                    product.RateCount = rateInfo.RateCount;
                }
            }

            return products;
        }

        public async Task<PagedResult<ProductCardModel>> Search(
        ProductFilterRequest request,
        CancellationToken cancellationToken = default)
        {
            if (request.Page <= 0) request.Page = 1;
            if (request.PageSize <= 0) request.PageSize = 10;

            var query = _shopContext.Products.Include(x => x.Category).AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().Replace("ي", "ی").Replace("ك", "ک");
                var likeTerm = $"%{term}%";
                query = query.Where(x =>
                    EF.Functions.Like(x.Name, likeTerm) ||
                    EF.Functions.Like(x.ShortDescription, likeTerm) ||
                    EF.Functions.Like(x.Specifications, likeTerm) || // جستجو در مشخصات فعلی (HTML/String)
                    EF.Functions.Like(x.Keywords, likeTerm) ||
                    EF.Functions.Like(x.Category.Name, likeTerm));
            }

            if (!string.IsNullOrWhiteSpace(request.Brand))
            {
                query = query.Where(x => x.Brand == request.Brand.Trim());
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryFullSlug))
            {
                query = query.Where(x => x.Category.FullSlug.StartsWith(request.CategoryFullSlug.Trim()));
            }

            var baseProductsRaw = await query.Select(p => new
            {
                p.Id,
                CategoryName = p.Category.Name,
                CategoryFullSlug = p.Category.FullSlug,
                p.Name,
                p.Picture,
                p.PictureAlt,
                p.PictureTitle,
                p.ShortDescription,
                p.Slug,
                p.Brand,
                p.Keywords,
                p.CategoryId,
                p.Specifications
            }).ToListAsync(cancellationToken);

            var baseProducts = baseProductsRaw.Select(p => new ProductCardModel
            {
                Id = p.Id,
                CategoryName = p.CategoryName,
                CategoryFullSlug = p.CategoryFullSlug,
                Name = p.Name,
                Picture = p.Picture,
                PictureAlt = p.PictureAlt,
                PictureTitle = p.PictureTitle,
                Slug = p.Slug,
                Brand = p.Brand,
                CategoryId = p.CategoryId,
                Specifications = !string.IsNullOrWhiteSpace(p.Specifications)
                ? JsonSerializer.Deserialize<ProductSpecificationModel>(p.Specifications, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                : new ProductSpecificationModel()
            }).ToList();

            if (!baseProducts.Any()) return new PagedResult<ProductCardModel>();

            var productIds = baseProducts.Select(x => x.Id).ToList();

            var inventory = await _inventoryContext.Inventory
                .Where(x => productIds.Contains(x.ProductId))
                .ToDictionaryAsync(x => x.ProductId, x => new { x.UnitPrice, x.InStock }, cancellationToken);

            var avgRates = await _commentContext.Comments
                .Where(x => productIds.Contains(x.RecordId) && x.IsConfirmed && !x.IsCanceled)
                .GroupBy(x => x.RecordId)
                .Select(g => new { ProductId = g.Key, Avg = g.Average(c => c.Rate), Count = g.Count() })
                .ToDictionaryAsync(x => x.ProductId, cancellationToken);

            var activeDiscounts = await _cacheService.GetOrCreateAsync("ActiveDiscounts",
                factory: async () => await _discountContext.Discounts.Include(x => x.Targets).AsNoTracking().Where(x => x.IsActive).ToListAsync(cancellationToken),
                absoluteExpiration: TimeSpan.FromMinutes(30), cancellationToken: cancellationToken);

            var now = DateTime.Now;
            var validDiscounts = activeDiscounts.Where(x => x.StartDate < now && x.EndDate > now).ToList();

            foreach (var product in baseProducts)
            {
                if (inventory.TryGetValue(product.Id, out var inv))
                {
                    product.Price = inv.UnitPrice.ToMoney();
                    product.DoublePrice = inv.UnitPrice;
                    product.IsInStock = inv.InStock;

                    var bestDiscount = validDiscounts
                        .Where(d => d.Targets.Any(t =>
                            t.TargetType == (DiscountTargetType)4 ||
                            (t.TargetType == (DiscountTargetType)1 && t.ProductId == product.Id) ||
                            (t.TargetType == (DiscountTargetType)3 && t.CategoryId == product.CategoryId)))
                        .OrderByDescending(x => x.Percentage)
                        .FirstOrDefault();

                    if (bestDiscount != null)
                    {
                        product.DiscountRate = bestDiscount.Percentage;
                        product.HasDiscount = true;
                        var discountAmount = Math.Round((inv.UnitPrice * bestDiscount.Percentage) / 100);
                        product.DoublePriceWithDiscount = inv.UnitPrice - discountAmount;
                        product.PriceWithDiscount = product.DoublePriceWithDiscount.ToMoney();
                    }
                    else
                    {
                        product.DoublePriceWithDiscount = inv.UnitPrice;
                    }
                }

                if (avgRates.TryGetValue(product.Id, out var rate))
                {
                    product.AvgRate = (float)rate.Avg;
                    product.RateCount = rate.Count;
                }
            }

            var filteredProducts = baseProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(request.CountryOfOrigin))
            {
                filteredProducts = filteredProducts.Where(x =>
                    x.Specifications != null &&
                    x.Specifications.CountryOfOrigin == request.CountryOfOrigin.Trim());
            }

            if (!string.IsNullOrWhiteSpace(request.ProductForm))
            {
                filteredProducts = filteredProducts.Where(x =>
                    x.Specifications != null &&
                    x.Specifications.ProductForm == request.ProductForm.Trim());
            }

            if (request.OnlyInStock)
                filteredProducts = filteredProducts.Where(x => x.IsInStock);

            if (request.HasDiscount)
                filteredProducts = filteredProducts.Where(x => x.HasDiscount);

            if (request.MinPrice.HasValue)
                filteredProducts = filteredProducts.Where(x => x.DoublePriceWithDiscount >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                filteredProducts = filteredProducts.Where(x => x.DoublePriceWithDiscount <= request.MaxPrice.Value);

            filteredProducts = request.SortBy switch
            {
                ProductSortOption.PriceAsc => filteredProducts.OrderByDescending(x => x.IsInStock).ThenBy(x => x.DoublePriceWithDiscount),
                ProductSortOption.PriceDesc => filteredProducts.OrderByDescending(x => x.IsInStock).ThenByDescending(x => x.DoublePriceWithDiscount),
                ProductSortOption.Popularity => filteredProducts.OrderByDescending(x => x.IsInStock).ThenByDescending(x => x.AvgRate).ThenByDescending(x => x.RateCount),
                ProductSortOption.HighestDiscount => filteredProducts.OrderByDescending(x => x.IsInStock).ThenByDescending(x => x.DiscountRate),
                ProductSortOption.MostViewed => filteredProducts.OrderByDescending(x => x.IsInStock).ThenByDescending(x => x.ViewCount),
                _ => filteredProducts.OrderByDescending(x => x.IsInStock).ThenByDescending(x => x.Id) // Newest default
            };

            var finalResult = filteredProducts.ToList();
            var totalCount = finalResult.Count;
            var pagedItems = finalResult.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new PagedResult<ProductCardModel>
            {
                Items = pagedItems,
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<List<ProductCardModel>> GetRandomRecommendations(int take = 10, CancellationToken cancellationToken = default)
        {
            if (take <= 0 || take > 50)
                take = 10;

            var now = DateTime.Now;

            // ==== inventory ====
            var inventory = await _inventoryContext.Inventory
                .Select(x => new ProductInventoryDto
                {
                    ProductId = x.ProductId,
                    UnitPrice = x.UnitPrice,
                    InStock = x.InStock
                })
                .ToListAsync(cancellationToken);

            var inStockProductIds = inventory
                .Where(x => x.InStock == true)
                .Select(x => x.ProductId)
                .ToList();

            if (!inStockProductIds.Any())
                return new List<ProductCardModel>();

            // ==== Rates ====
            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.RecordId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync(cancellationToken);


            var rand = new Random();

            if (take > inStockProductIds.Count)
                take = inStockProductIds.Count;

            var randomIds = inStockProductIds
                .OrderBy(x => rand.Next())
                .Take(take)
                .ToList();

            var activeDiscounts = await _discountContext.Discounts
                .Include(x => x.Targets)
                .Where(x => x.IsActive && x.Type == (DiscountType)1
                && x.ApplyType == (DiscountApplyType)1) // 1: Automatic, 1: OnProduct
                .Where(x => x.StartDate < now && x.EndDate > now)
                .ToListAsync(cancellationToken);

            var products = await _shopContext.Products
                .Include(x => x.Category)
                .Where(x => randomIds.Contains(x.Id))
                .Select(product => new ProductCardModel
                {
                    Id = product.Id,
                    CategoryId = product.CategoryId,
                    Brand = product.Brand,
                    CategoryName = product.Category.Name,
                    CategoryFullSlug = product.Category.FullSlug,
                    Name = product.Name,
                    Picture = product.Picture,
                    PictureAlt = product.PictureAlt,
                    PictureTitle = product.PictureTitle,
                    Slug = product.Slug,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // ===== merge inventory & discount =====
            foreach (var product in products)
            {
                var inv = inventory.FirstOrDefault(x => x.ProductId == product.Id);

                if (inv != null)
                {
                    var price = inv.UnitPrice;
                    product.Price = price.ToMoney();
                    product.IsInStock = inv.InStock;

                    var bestDiscount = activeDiscounts
                        .Where(d => d.Targets.Any(t =>
                            t.TargetType == (DiscountTargetType)4 || // اعمال به همه محصولات
                            (t.TargetType == (DiscountTargetType)1 && t.ProductId == product.Id) || // تخفیف مستقیم محصول
                            (t.TargetType == (DiscountTargetType)3 && t.CategoryId == product.CategoryId) || // تخفیف روی دسته بندی
                            (t.TargetType == (DiscountTargetType)2 && !string.IsNullOrEmpty(product.Brand) && t.BrandName?.ToLower() == product.Brand.ToLower()) // تخفیف روی برند
                        ))
                        .OrderByDescending(x => x.Percentage)
                        .FirstOrDefault();

                    if (bestDiscount != null)
                    {
                        product.DiscountRate = bestDiscount.Percentage;
                        product.DiscountExpireDate = bestDiscount.EndDate?.ToFarsi();
                        product.HasDiscount = true;

                        var amount = Math.Round((inv.UnitPrice * bestDiscount.Percentage) / 100);
                        product.PriceWithDiscount = (inv.UnitPrice - amount).ToMoney();
                    }
                    else
                    {
                        product.PriceWithDiscount = product.Price;
                        product.HasDiscount = false;
                    }

                    var rateInfo = avgRates.FirstOrDefault(x => x.ProductId == product.Id);
                    if (rateInfo != null)
                    {
                        product.AvgRate = (float)rateInfo.AvgRate;
                        product.RateCount = rateInfo.RateCount;
                    }
                }
            }

            return products;
        }

        public async Task<List<ProductCardModel>> GetTopRatedProducts(int take = 10, CancellationToken cancellationToken = default)
        {
            if (take <= 0 || take > 50)
                take = 10;

            var now = DateTime.Now;

            // ---------- Load Inventory ----------
            var inventory = await _inventoryContext.Inventory
                .Select(x => new ProductInventoryDto
                {
                    ProductId = x.ProductId,
                    UnitPrice = x.UnitPrice,
                    InStock = x.InStock
                })
                .ToListAsync(cancellationToken);

            var inStockProductIds = inventory
                .Where(x => x.InStock)
                .Select(x => x.ProductId)
                .ToList();

            if (!inStockProductIds.Any())
                return new List<ProductCardModel>();

            // ---------- Calculate AvgRate from Comments ----------
            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.RecordId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync(cancellationToken);

            var popularIds = avgRates
                .Where(x => inStockProductIds.Contains(x.ProductId))
                .OrderByDescending(x => x.AvgRate)
                .ThenByDescending(x => x.RateCount)
                .Take(take)
                .Select(x => x.ProductId)
                .ToList();

            if (!popularIds.Any())
                return new List<ProductCardModel>();

            var activeDiscounts = await _discountContext.Discounts
                .Include(x => x.Targets)
                .Where(x => x.IsActive && x.Type == (DiscountType)1
                && x.ApplyType == (DiscountApplyType)1) // 1: Automatic, 1: OnProduct
                .Where(x => x.StartDate < now && x.EndDate > now)
                .ToListAsync(cancellationToken);

            // ---------- Load Product Info ----------
            var products = await _shopContext.Products
                .Include(x => x.Category)
                .Where(x => popularIds.Contains(x.Id))
                .Select(product => new ProductCardModel
                {
                    Id = product.Id,
                    CategoryId = product.CategoryId,
                    Brand = product.Brand,
                    Name = product.Name,
                    Slug = product.Slug,
                    CategoryName = product.Category.Name,
                    CategoryFullSlug = product.Category.FullSlug,
                    Picture = product.Picture,
                    PictureAlt = product.PictureAlt,
                    PictureTitle = product.PictureTitle
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // ---------- Merge Inventory & Discounts ----------

            foreach (var product in products)
            {
                var inv = inventory.FirstOrDefault(x => x.ProductId == product.Id);
                if (inv != null)
                {
                    var price = inv.UnitPrice;
                    product.IsInStock = inv.InStock;
                    product.Price = price.ToMoney();

                    var bestDiscount = activeDiscounts
                        .Where(d => d.Targets.Any(t =>
                            t.TargetType == (DiscountTargetType)4 || // اعمال به همه محصولات
                            (t.TargetType == (DiscountTargetType)1 && t.ProductId == product.Id) || // تخفیف مستقیم محصول
                            (t.TargetType == (DiscountTargetType)3 && t.CategoryId == product.CategoryId) || // تخفیف روی دسته بندی
                            (t.TargetType == (DiscountTargetType)2 && !string.IsNullOrEmpty(product.Brand) && t.BrandName?.ToLower() == product.Brand.ToLower()) // تخفیف روی برند
                        ))
                        .OrderByDescending(x => x.Percentage)
                        .FirstOrDefault();

                    if (bestDiscount != null)
                    {
                        product.DiscountRate = bestDiscount.Percentage;
                        product.DiscountExpireDate = bestDiscount.EndDate?.ToFarsi();
                        product.HasDiscount = true;

                        var amount = Math.Round((inv.UnitPrice * bestDiscount.Percentage) / 100);
                        product.PriceWithDiscount = (inv.UnitPrice - amount).ToMoney();
                    }
                    else
                    {
                        product.PriceWithDiscount = product.Price;
                        product.HasDiscount = false;
                    }
                }

                var rateInfo = avgRates.FirstOrDefault(x => x.ProductId == product.Id);
                if (rateInfo != null)
                {
                    product.AvgRate = (float)rateInfo.AvgRate;
                    product.RateCount = rateInfo.RateCount;
                }
            }

            return products
                .OrderByDescending(x => x.AvgRate)
                .ThenByDescending(x => x.RateCount)
                .ToList();
        }

        public async Task<List<ProductCardModel>> GetMostViewedProducts(int take = 10, CancellationToken cancellationToken = default)
        {
            if (take <= 0 || take > 50)
                take = 10;

            var now = DateTime.Now;

            // ---------- Load Inventory ----------
            var inventory = await _inventoryContext.Inventory
                .Select(x => new ProductInventoryDto
                {
                    ProductId = x.ProductId,
                    UnitPrice = x.UnitPrice,
                    InStock = x.InStock
                })
                .ToListAsync(cancellationToken);

            var inStockProductIds = inventory
                .Where(x => x.InStock)
                .Select(x => x.ProductId)
                .ToList();

            if (!inStockProductIds.Any())
                return new List<ProductCardModel>();

            // ---------- Calculate AvgRate from Comments ----------
            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.RecordId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync(cancellationToken);

            var popularIds = await _shopContext.Products
                .Where(x => inStockProductIds.Contains(x.Id))
                .OrderByDescending(x => x.ViewCount)
                .Take(take)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (!popularIds.Any())
                return new List<ProductCardModel>();

            var activeDiscounts = await _discountContext.Discounts
                .Include(x => x.Targets)
                .Where(x => x.IsActive && x.Type == (DiscountType)1
                && x.ApplyType == (DiscountApplyType)1) // 1: Automatic, 1: OnProduct
                .Where(x => x.StartDate < now && x.EndDate > now)
                .ToListAsync(cancellationToken);

            // ---------- Load Product Info ----------
            var products = await _shopContext.Products
                .Include(x => x.Category)
                .Where(x => popularIds.Contains(x.Id))
                .Select(product => new ProductCardModel
                {
                    Id = product.Id,
                    CategoryId = product.CategoryId,
                    Brand = product.Brand,
                    Name = product.Name,
                    Slug = product.Slug,
                    CategoryName = product.Category.Name,
                    CategoryFullSlug = product.Category.FullSlug,
                    Picture = product.Picture,
                    PictureAlt = product.PictureAlt,
                    PictureTitle = product.PictureTitle,
                    ViewCount = product.ViewCount
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // ---------- Merge Inventory & Discounts ----------

            foreach (var product in products)
            {
                var inv = inventory.FirstOrDefault(x => x.ProductId == product.Id);

                if (inv != null)
                {
                    var price = inv.UnitPrice;
                    product.IsInStock = inv.InStock;
                    product.Price = price.ToMoney();

                    var bestDiscount = activeDiscounts
                        .Where(d => d.Targets.Any(t =>
                            t.TargetType == (DiscountTargetType)4 || // اعمال به همه محصولات
                            (t.TargetType == (DiscountTargetType)1 && t.ProductId == product.Id) || // تخفیف مستقیم محصول
                            (t.TargetType == (DiscountTargetType)3 && t.CategoryId == product.CategoryId) || // تخفیف روی دسته بندی
                            (t.TargetType == (DiscountTargetType)2 && !string.IsNullOrEmpty(product.Brand) && t.BrandName?.ToLower() == product.Brand.ToLower()) // تخفیف روی برند
                        ))
                        .OrderByDescending(x => x.Percentage)
                        .FirstOrDefault();

                    if (bestDiscount != null)
                    {
                        product.DiscountRate = bestDiscount.Percentage;
                        product.DiscountExpireDate = bestDiscount.EndDate?.ToFarsi();
                        product.HasDiscount = true;

                        var amount = Math.Round((inv.UnitPrice * bestDiscount.Percentage) / 100);
                        product.PriceWithDiscount = (inv.UnitPrice - amount).ToMoney();
                    }
                    else
                    {
                        product.PriceWithDiscount = product.Price;
                        product.HasDiscount = false;
                    }
                }

                var rateInfo = avgRates.FirstOrDefault(x => x.ProductId == product.Id);
                if (rateInfo != null)
                {
                    product.AvgRate = (float)rateInfo.AvgRate;
                    product.RateCount = rateInfo.RateCount;
                }
            }

            return products
                .OrderByDescending(x => x.ViewCount)
                .ThenByDescending(x => x.AvgRate)
                .ThenByDescending(x => x.RateCount)
                .ToList();
        }

        public async Task<List<FaveProductsCardModel>> GetUserFavoriteList(long userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;

            var favProducts = await _shopContext.UserFavoriteProducts
                .Where(x => x.UserId == userId)
                .ToDictionaryAsync(x => x.ProductId, x => x.CreationDate, cancellationToken);

            var favProductIds = favProducts.Keys.ToList();

            if (!favProductIds.Any())
                return new List<FaveProductsCardModel>();

            var activeDiscounts = await _discountContext.Discounts
                .Include(x => x.Targets)
                .Where(x => x.IsActive && x.Type == (DiscountType)1
                && x.ApplyType == (DiscountApplyType)1) // 1: Automatic, 1: OnProduct
                .Where(x => x.StartDate < now && x.EndDate > now)
                .ToListAsync(cancellationToken);

            var products = await _shopContext.Products
                .Where(x => favProductIds.Contains(x.Id))
                .Include(x => x.Category)
                .Select(product => new FaveProductsCardModel
                {
                    Id = product.Id,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    CategoryFullSlug = product.Category.FullSlug,
                    Name = product.Name,
                    Picture = product.Picture,
                    PictureAlt = product.PictureAlt,
                    PictureTitle = product.PictureTitle,
                    Slug = product.Slug,
                    Brand = product.Brand,
                })
                .ToListAsync(cancellationToken: cancellationToken);

            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.RecordId)
                .Select(g => new { ProductId = g.Key, AvgRate = g.Average(c => c.Rate), Count = g.Count() })
                .ToDictionaryAsync(x => x.ProductId, cancellationToken);

            var inventory = await _inventoryContext.Inventory
                .Where(x => favProductIds.Contains(x.ProductId))
                .ToDictionaryAsync(x => x.ProductId, cancellationToken);

            foreach (var p in products)
            {
                var favDate = favProducts[p.Id];
                p.CurrentUserFavedDate = favDate;
                p.CurrentUserFavedDateStr = favDate.ToFarsi();

                if (inventory.TryGetValue(p.Id, out var inv))
                {
                    p.Price = inv.UnitPrice.ToMoney();
                    p.IsInStock = inv.InStock;

                    var bestDiscount = activeDiscounts
                        .Where(d => d.Targets.Any(t =>
                            t.TargetType == (DiscountTargetType)4 || // اعمال به همه محصولات
                            (t.TargetType == (DiscountTargetType)1 && t.ProductId == p.Id) || // تخفیف مستقیم محصول
                            (t.TargetType == (DiscountTargetType)3 && t.CategoryId == p.CategoryId) || // تخفیف روی دسته بندی
                            (t.TargetType == (DiscountTargetType)2 && !string.IsNullOrEmpty(p.Brand) && t.BrandName?.ToLower() == p.Brand.ToLower()) // تخفیف روی برند
                        ))
                        .OrderByDescending(x => x.Percentage)
                        .FirstOrDefault();

                    if (bestDiscount != null)
                    {
                        p.DiscountRate = bestDiscount.Percentage;
                        p.DiscountExpireDate = bestDiscount.EndDate?.ToFarsi();
                        p.HasDiscount = true;

                        var amount = Math.Round((inv.UnitPrice * bestDiscount.Percentage) / 100);
                        p.PriceWithDiscount = (inv.UnitPrice - amount).ToMoney();
                    }
                    else
                    {
                        p.PriceWithDiscount = p.Price;
                        p.HasDiscount = false;
                    }
                }

                if (avgRates.TryGetValue(p.Id, out var rate))
                {
                    p.AvgRate = (float)rate.AvgRate;
                    p.RateCount = rate.Count;
                }
            }

            return products.OrderByDescending(x => x.CurrentUserFavedDate).ToList();
        }

        public async Task<SearchByBrandResult?> GetProductsByBrand(string brandName, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(brandName))
                return null;

            var now = DateTime.Now;
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _shopContext.Products
                .AsNoTracking()
                .Where(x => x.Brand.ToLower() == brandName.ToLower());

            var totalCount = await query.CountAsync(cancellationToken);

            var activeDiscounts = await _discountContext.Discounts
                .Include(x => x.Targets)
                .Where(x => x.IsActive && x.Type == (DiscountType)1
                && x.ApplyType == (DiscountApplyType)1) // 1: Automatic, 1: OnProduct
                .Where(x => x.StartDate < now && x.EndDate > now)
                .ToListAsync(cancellationToken);

            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.RecordId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync(cancellationToken);

            var products = await query.OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(product => new ProductQueryModel
                {
                    Id = product.Id,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    CategoryFullSlug = product.Category.FullSlug,
                    Name = product.Name,
                    Picture = product.Picture,
                    PictureAlt = product.PictureAlt,
                    PictureTitle = product.PictureTitle,
                    ShortDescription = product.ShortDescription,
                    Slug = product.Slug,
                    Brand = product.Brand,
                    Keywords = product.Keywords,
                    MetaDescription = product.MetaDescription,
                }).ToListAsync(cancellationToken);

            var categories = await query
                .Select(x => x.Category.Name)
                .Distinct()
                .ToListAsync(cancellationToken);

            var productIds = products.Select(x => x.Id).ToList();

            var inventory = await _inventoryContext.Inventory
                .Where(x => productIds.Contains(x.ProductId))
                .Select(x => new { x.ProductId, x.UnitPrice, x.InStock })
                .ToDictionaryAsync(x => x.ProductId, x => new { x.UnitPrice, x.InStock }, cancellationToken);

            foreach (var product in products)
            {
                if (inventory.TryGetValue(product.Id, out var inv))
                {
                    product.Price = inv.UnitPrice.ToMoney();
                    product.IsInStock = inv.InStock;

                    var bestDiscount = activeDiscounts
                        .Where(d => d.Targets.Any(t =>
                            t.TargetType == (DiscountTargetType)4 || // اعمال به همه
                            (t.TargetType == (DiscountTargetType)1 && t.ProductId == product.Id) || // تطبیق محصول
                            (t.TargetType == (DiscountTargetType)3 && t.CategoryId == product.CategoryId) || // تطبیق دسته
                            (t.TargetType == (DiscountTargetType)2 && t.BrandName?.ToLower() == product.Brand.ToLower()) // تطبیق برند
                        ))
                        .OrderByDescending(x => x.Percentage)
                        .FirstOrDefault();

                    if (bestDiscount != null)
                    {
                        int rate = bestDiscount.Percentage;
                        product.DiscountRate = rate;
                        product.DiscountExpireDate = bestDiscount.EndDate?.ToFarsi();
                        product.HasDiscount = true;

                        var discountAmount = Math.Round((inv.UnitPrice * rate) / 100);
                        product.PriceWithDiscount = (inv.UnitPrice - discountAmount).ToMoney();
                    }
                    else
                    {
                        product.PriceWithDiscount = product.Price;
                        product.HasDiscount = false;
                    }

                    var rateInfo = avgRates.FirstOrDefault(x => x.ProductId == product.Id);
                    if (rateInfo != null)
                    {
                        product.AvgRate = (float)rateInfo.AvgRate;
                        product.RateCount = rateInfo.RateCount;
                    }
                }
            }

            return new SearchByBrandResult
            {
                Products = new PagedResult<ProductQueryModel>
                {
                    Items = products,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                },
                Categories = categories
            };
        }

        public async Task<bool> Exists(long productId, CancellationToken cancellationToken = default)
        {
            return await _shopContext.Products.AnyAsync(
                x => x.Id == productId, cancellationToken);
        }
    }
}