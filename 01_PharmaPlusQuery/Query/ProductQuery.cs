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

namespace _01_PharmaPlusQuery.Query
{
    public class ProductQuery : IProductQuery
    {
        private readonly ShopContext _shopContext;
        private readonly InventoryContext _inventoryContext;
        private readonly DiscountContext _discountContext;
        private readonly CommentContext _commentContext;
        //private readonly ICacheService _cacheService;

        public ProductQuery(ShopContext context, InventoryContext inventoryContext,
        DiscountContext discountContext, CommentContext commentContext, ICacheService cache)
        {
            _shopContext = context;
            _discountContext = discountContext;
            _inventoryContext = inventoryContext;
            _commentContext = commentContext;
            //_cacheService = cache;
        }

        public async Task<ProductQueryModel> GetProductDetails(string productSlug)
        {
            // -----------------------------
            // 1) Get Product Base Data
            // -----------------------------
            var productBase = await _shopContext.Products
                .AsNoTracking()
                .Where(x => x.Slug == productSlug)
                .Select(x => new
                {
                    x.Id,
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
                .FirstOrDefaultAsync();

            if (productBase == null)
                return null;

            long productId = productBase.Id;
            var cutoffTime = DateTime.Now.AddHours(-48);

            // Pictures (MANUAL SPLIT)
            var picturesTask = _shopContext.ProductPictures
                .AsNoTracking()
                .Where(p => p.ProductId == productId && !p.IsRemoved)
                .Select(p => new ProductPictureQueryModel
                {
                    Picture = p.Picture,
                    PictureAlt = p.PictureAlt,
                    PictureTitle = p.PictureTitle,
                    ProductId = productId
                })
                .ToListAsync();
            var pictures = await picturesTask;

            // Recent Views (Real-time)
            var recentViewsTask = _shopContext.ProductViews
                .AsNoTracking()
                .CountAsync(v => v.ProductId == productId && v.CreationDate >= cutoffTime);
            var recentViews = await recentViewsTask;

            // -----------------------------
            // 2) Parallel Data Fetching
            // -----------------------------

            // Inventory (Real-time)
            var inventoryTask = _inventoryContext.Inventory
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == productId);

            // Discount (Real-time)
            var discountTask = _discountContext.CustomerDiscounts
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ProductId == productId &&
                    x.StartDate <= DateTime.Now &&
                    x.EndDate >= DateTime.Now);

            // Comments (Real-time)
            var commentsRawTask = _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.OwnerRecordId == productId)
                .AsNoTracking()
                .Select(x => new CommentQueryModel
                {
                    Id = x.Id,
                    Message = x.Message,
                    Username = x.Username,
                    CreationDate = x.CreationDate.ToFarsi().ToPersianNumber(),
                    Reply = x.AdminReply,
                    DislikeCount = x.DislikeCount,
                    LikeCount = x.LikeCount,
                    Rate = x.Rate,
                    ReplyDate = x.AdminReplyDate.ToFarsi().ToPersianNumber(),
                    IsBuyer = x.IsBuyer,
                    UserId = long.Parse(x.UserIdentifier),
                    LastModifedDate = x.LastModified.ToFarsi().ToPersianNumber()
                })
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            await Task.WhenAll(inventoryTask, discountTask, commentsRawTask);

            var inventory = inventoryTask.Result;
            var discount = discountTask.Result;
            var commentsRaw = commentsRawTask.Result;

            // -----------------------------
            // 3) Calculate Price and Stats
            // -----------------------------
            double unitPrice = inventory?.UnitPrice ?? 0;
            bool hasDiscount = discount != null && discount.DiscountRate > 0;
            int discountedQty = discount.ProductCount;
            double priceWithDiscount = hasDiscount
                ? Math.Round(unitPrice * (100 - discount.DiscountRate) / 100)
                : unitPrice;

            int rateCount = commentsRaw.Count(c => c.Rate >= 1 && c.Rate <= 5);
            int commentCount = commentsRaw.Count(c => !string.IsNullOrWhiteSpace(c.Message));
            float avgRate = rateCount == 0
                ? 0
                : (float)commentsRaw.Where(c => c.Rate >= 1 && c.Rate <= 5).Sum(c => c.Rate) / rateCount;

            var comments = commentsRaw.Where(c => !string.IsNullOrWhiteSpace(c.Message)).ToList();

            // -----------------------------
            // 4) Build Final Model
            // -----------------------------

            return new ProductQueryModel
            {
                Id = productBase.Id,
                Name = productBase.Name,
                Code = productBase.Code,
                Brand = productBase.Brand,
                Description = productBase.Description,
                ShortDescription = productBase.ShortDescription,
                MetaDescription = productBase.MetaDescription,
                Specifications = productBase.Specifications,
                Keywords = productBase.Keywords,
                Slug = productBase.Slug,
                ViewCount = productBase.ViewCount,

                CategoryName = productBase.CategoryName,
                CategorySlug = productBase.CategorySlug,
                CategoryFullSlug = productBase.CategoryFullSlug,

                Picture = productBase.Picture,
                PictureAlt = productBase.PictureAlt,
                PictureTitle = productBase.PictureTitle,

                Pictures = pictures,

                Price = unitPrice.ToMoney().ToPersianNumber(),
                DoublePrice = unitPrice,
                HasDiscount = hasDiscount,
                DiscountRate = hasDiscount ? discount.DiscountRate : 0,
                DiscountExpireDate = hasDiscount ? discount.EndDate.ToFarsi().ToPersianNumber() : "",
                PriceWithDiscount = priceWithDiscount.ToMoney().ToPersianNumber(),
                DiscountedQty = discountedQty,
                IsInStock = inventory != null && inventory.InStock,
                InStockQty = inventory.CalculateCurrentCount(),

                Comments = comments,
                CommentCount = commentCount,
                RateCount = rateCount,
                AvgRate = avgRate,

                RecentViewsLabel = "+" + (Math.Ceiling(recentViews / 10.0) * 10) + " بازدید در 48 ساعت اخیر"
            };
        }

        public async Task<List<ProductQueryModel>> GetLatestArrivals()
        {
            var now = DateTime.Now;

            // ===== Load Inventory & Discounts & Rates =====
            var inventory = await _inventoryContext.Inventory
            .Select(x => new ProductInventoryDto
            { ProductId = x.ProductId, UnitPrice = x.UnitPrice, InStock = x.InStock })
            .ToListAsync();

            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => x.StartDate < now && x.EndDate > now)
                .Select(x => new ProductDiscountDto { ProductId = x.ProductId, DiscountRate = x.DiscountRate, EndDate = x.EndDate })
                .ToListAsync();

            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.OwnerRecordId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync();

            var products = await _shopContext.Products.Include(x => x.Category)
                .Select(product => new ProductQueryModel
                {
                    Id = product.Id,
                    CategoryName = product.Category.Name,
                    CategoryFullSlug = product.Category.FullSlug,
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
                    product.Price = price.ToMoney().ToPersianNumber();
                    product.IsInStock = productInventory.InStock;
                    var discount = discounts.FirstOrDefault(x => x.ProductId == product.Id);
                    if (discount != null)
                    {
                        int discountRate = discount.DiscountRate;
                        product.DiscountRate = discountRate;
                        product.HasDiscount = discountRate > 0;
                        var discountAmount = Math.Round((price * discountRate) / 100);
                        product.PriceWithDiscount = (price - discountAmount).ToMoney().ToPersianNumber();
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

        public async Task<PagedResult<ProductQueryModel>> Search(string value, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _shopContext.Products
            .Include(x => x.Category)
            .AsQueryable()
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
                    EF.Functions.Like(x.Brand.ToLower(), $"%{value}%") ||
                    EF.Functions.Like(x.Category.Name.ToLower(), $"%{value}%"));
            }

            var totalCount = await query.CountAsync();

            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.OwnerRecordId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync();

            var products = await query.OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(product => new ProductQueryModel
                {
                    Id = product.Id,
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
                }).ToListAsync();


            var productIds = products.Select(x => x.Id).ToList();

            var inventory = await _inventoryContext.Inventory
                .Where(x => productIds.Contains(x.ProductId))
                .Select(x => new { x.ProductId, x.UnitPrice, x.InStock })
                .ToDictionaryAsync(x => x.ProductId, x => new { x.UnitPrice, x.InStock });

            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => productIds.Contains(x.ProductId)
                            && x.StartDate < DateTime.Now
                            && x.EndDate > DateTime.Now)
                .Select(x => new { x.ProductId, x.DiscountRate, x.EndDate })
                .ToDictionaryAsync(x => x.ProductId, x => new { x.DiscountRate, x.EndDate });

            foreach (var product in products)
            {
                if (inventory.TryGetValue(product.Id, out var inv))
                {
                    product.Price = inv.UnitPrice.ToMoney().ToPersianNumber();
                    product.IsInStock = inv.InStock;

                    if (discounts.TryGetValue(product.Id, out var discount) && discount.DiscountRate > 0)
                    {
                        int rate = discount.DiscountRate;
                        product.DiscountRate = rate;
                        product.DiscountExpireDate = discount.EndDate.ToFarsi().ToPersianNumber();
                        product.HasDiscount = true;

                        var discountAmount = Math.Round((inv.UnitPrice * rate) / 100);
                        product.PriceWithDiscount = (inv.UnitPrice - discountAmount).ToMoney().ToPersianNumber();
                    }

                    var rateInfo = avgRates.FirstOrDefault(x => x.ProductId == product.Id);
                    if (rateInfo != null)
                    {
                        product.AvgRate = (float)rateInfo.AvgRate;
                        product.RateCount = rateInfo.RateCount;
                    }
                }
            }

            return new PagedResult<ProductQueryModel>
            {
                Items = products,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<List<ProductQueryModel>> GetRandomRecommendations(int take = 10)
        {
            if (take <= 0)
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
                .ToListAsync();

            var inStockProductIds = inventory
                .Where(x => x.InStock == true)
                .Select(x => x.ProductId)
                .ToList();

            if (!inStockProductIds.Any())
                return new List<ProductQueryModel>();

            // ==== discount ====
            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => x.StartDate < now && x.EndDate > now)
                .Select(x => new ProductDiscountDto
                {
                    ProductId = x.ProductId,
                    DiscountRate = x.DiscountRate,
                    EndDate = x.EndDate
                })
                .ToListAsync();

            // ==== Rates ====
            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.OwnerRecordId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync();


            var rand = new Random();

            if (take > inStockProductIds.Count)
                take = inStockProductIds.Count;

            var randomIds = inStockProductIds
                .OrderBy(x => rand.Next())
                .Take(take)
                .ToList();

            var products = await _shopContext.Products
                .Include(x => x.Category)
                .Where(x => randomIds.Contains(x.Id))
                .Select(product => new ProductQueryModel
                {
                    Id = product.Id,
                    CategoryName = product.Category.Name,
                    CategoryFullSlug = product.Category.FullSlug,
                    Name = product.Name,
                    Picture = product.Picture,
                    PictureAlt = product.PictureAlt,
                    PictureTitle = product.PictureTitle,
                    Slug = product.Slug,
                    MetaDescription = product.MetaDescription,
                })
                .AsNoTracking()
                .ToListAsync();

            // ===== merge inventory & discount =====
            foreach (var product in products)
            {
                var inv = inventory.FirstOrDefault(x => x.ProductId == product.Id);

                if (inv != null)
                {
                    var price = inv.UnitPrice;
                    product.Price = price.ToMoney().ToPersianNumber();
                    product.IsInStock = inv.InStock;

                    var disc = discounts.FirstOrDefault(x => x.ProductId == product.Id);
                    if (disc != null)
                    {
                        int rate = disc.DiscountRate;
                        product.DiscountRate = rate;
                        product.HasDiscount = rate > 0;

                        var discountAmount = Math.Round((price * rate) / 100);
                        product.PriceWithDiscount = (price - discountAmount)
                            .ToMoney()
                            .ToPersianNumber();
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

        public async Task<List<ProductQueryModel>> GetTopRatedProducts(int take = 10)
        {
            if (take <= 0)
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
                .ToListAsync();

            var inStockProductIds = inventory
                .Where(x => x.InStock)
                .Select(x => x.ProductId)
                .ToList();

            if (!inStockProductIds.Any())
                return new List<ProductQueryModel>();

            // ---------- Calculate AvgRate from Comments ----------
            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.OwnerRecordId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync();

            var popularIds = avgRates
                .Where(x => inStockProductIds.Contains(x.ProductId))
                .OrderByDescending(x => x.AvgRate)
                .ThenByDescending(x => x.RateCount)
                .Take(take)
                .Select(x => x.ProductId)
                .ToList();

            if (!popularIds.Any())
                return new List<ProductQueryModel>();

            // ---------- Load Product Info ----------
            var products = await _shopContext.Products
                .Include(x => x.Category)
                .Where(x => popularIds.Contains(x.Id))
                .Select(product => new ProductQueryModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Slug = product.Slug,
                    MetaDescription = product.MetaDescription,
                    CategoryName = product.Category.Name,
                    CategoryFullSlug = product.Category.FullSlug,
                    Picture = product.Picture,
                    PictureAlt = product.PictureAlt,
                    PictureTitle = product.PictureTitle
                })
                .AsNoTracking()
                .ToListAsync();

            // ---------- Merge Inventory & Discounts ----------
            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => x.StartDate <= now && x.EndDate >= now)
                .Select(x => new ProductDiscountDto
                {
                    ProductId = x.ProductId,
                    DiscountRate = x.DiscountRate,
                    EndDate = x.EndDate
                })
                .ToListAsync();

            foreach (var product in products)
            {
                var inv = inventory.FirstOrDefault(x => x.ProductId == product.Id);
                var disc = discounts.FirstOrDefault(x => x.ProductId == product.Id);

                if (inv != null)
                {
                    var price = inv.UnitPrice;
                    product.IsInStock = inv.InStock;
                    product.Price = price.ToMoney().ToPersianNumber();

                    if (disc != null && disc.DiscountRate > 0)
                    {
                        product.DiscountRate = disc.DiscountRate;
                        product.HasDiscount = true;

                        var discountAmount = Math.Round((price * disc.DiscountRate) / 100);
                        product.PriceWithDiscount = (price - discountAmount)
                            .ToMoney().ToPersianNumber();
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

        public async Task<List<ProductQueryModel>> GetMostViewedProducts(int take = 10)
        {
            if (take <= 0)
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
                .ToListAsync();

            var inStockProductIds = inventory
                .Where(x => x.InStock)
                .Select(x => x.ProductId)
                .ToList();

            if (!inStockProductIds.Any())
                return new List<ProductQueryModel>();

            // ---------- Calculate AvgRate from Comments ----------
            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.OwnerRecordId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync();

            var popularIds = await _shopContext.Products
                .Where(x => inStockProductIds.Contains(x.Id))
                .OrderByDescending(x => x.ViewCount)
                .Take(take)
                .Select(x => x.Id)
                .ToListAsync();

            if (!popularIds.Any())
                return new List<ProductQueryModel>();

            // ---------- Load Product Info ----------
            var products = await _shopContext.Products
                .Include(x => x.Category)
                .Where(x => popularIds.Contains(x.Id))
                .Select(product => new ProductQueryModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Slug = product.Slug,
                    MetaDescription = product.MetaDescription,
                    CategoryName = product.Category.Name,
                    CategoryFullSlug = product.Category.FullSlug,
                    Picture = product.Picture,
                    PictureAlt = product.PictureAlt,
                    PictureTitle = product.PictureTitle,
                    ViewCount = product.ViewCount
                })
                .AsNoTracking()
                .ToListAsync();

            // ---------- Merge Inventory & Discounts ----------
            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => x.StartDate <= now && x.EndDate >= now)
                .Select(x => new ProductDiscountDto
                {
                    ProductId = x.ProductId,
                    DiscountRate = x.DiscountRate,
                    EndDate = x.EndDate
                })
                .ToListAsync();

            foreach (var product in products)
            {
                var inv = inventory.FirstOrDefault(x => x.ProductId == product.Id);
                var disc = discounts.FirstOrDefault(x => x.ProductId == product.Id);

                if (inv != null)
                {
                    var price = inv.UnitPrice;
                    product.IsInStock = inv.InStock;
                    product.Price = price.ToMoney().ToPersianNumber();

                    if (disc != null && disc.DiscountRate > 0)
                    {
                        product.DiscountRate = disc.DiscountRate;
                        product.HasDiscount = true;

                        var discountAmount = Math.Round((price * disc.DiscountRate) / 100);
                        product.PriceWithDiscount = (price - discountAmount)
                            .ToMoney().ToPersianNumber();
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

        public async Task<List<ProductQueryModel>> GetUserFavoriteList(string accountId)
        {
            var favProducts = await _shopContext.UserFavoriteProducts
                .Where(x => x.UserId == accountId)
                .ToDictionaryAsync(x => x.ProductId, x => x.CreationDate);

            var favProductIds = favProducts.Keys.ToList();

            if (!favProductIds.Any())
                return new List<ProductQueryModel>();

            var products = await _shopContext.Products
                .Where(x => favProductIds.Contains(x.Id))
                .Include(x => x.Category)
                .Select(product => new ProductQueryModel
                {
                    Id = product.Id,
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
                })
                .ToListAsync();

            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.OwnerRecordId)
                .Select(g => new { ProductId = g.Key, AvgRate = g.Average(c => c.Rate), Count = g.Count() })
                .ToDictionaryAsync(x => x.ProductId);

            var inventory = await _inventoryContext.Inventory
                .Where(x => favProductIds.Contains(x.ProductId))
                .ToDictionaryAsync(x => x.ProductId);

            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => favProductIds.Contains(x.ProductId)
                            && x.StartDate < DateTime.Now
                            && x.EndDate > DateTime.Now)
                .ToDictionaryAsync(x => x.ProductId);

            foreach (var p in products)
            {
                var favDate = favProducts[p.Id];
                p.UserFavedDate = favDate;
                p.UserFavedDateStr = favDate.ToFarsi().ToPersianNumber();

                if (inventory.TryGetValue(p.Id, out var inv))
                {
                    p.Price = inv.UnitPrice.ToMoney().ToPersianNumber();
                    p.IsInStock = inv.InStock;

                    if (discounts.TryGetValue(p.Id, out var discount))
                    {
                        p.DiscountRate = discount.DiscountRate;
                        p.DiscountExpireDate = discount.EndDate.ToFarsi().ToPersianNumber();
                        p.HasDiscount = true;

                        var amount = Math.Round((inv.UnitPrice * discount.DiscountRate) / 100);
                        p.PriceWithDiscount = (inv.UnitPrice - amount).ToMoney().ToPersianNumber();
                    }
                }

                if (avgRates.TryGetValue(p.Id, out var rate))
                {
                    p.AvgRate = (float)rate.AvgRate;
                    p.RateCount = rate.Count;
                }
            }

            return products.OrderByDescending(x => x.UserFavedDate).ToList();
        }

        public async Task<SearchByBrandResult> GetProductsByBrand(string brandName, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(brandName))
                return null;

            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _shopContext.Products
                .AsNoTracking()
                .Where(x => x.Brand.ToLower() == brandName.ToLower());

            var totalCount = await query.CountAsync();

            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Product)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.OwnerRecordId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync();

            var products = await query.OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(product => new ProductQueryModel
                {
                    Id = product.Id,
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
                }).ToListAsync();

            var categories = await query
                .Select(x => x.Category.Name)
                .Distinct()
                .ToListAsync();

            var productIds = products.Select(x => x.Id).ToList();

            var inventory = await _inventoryContext.Inventory
                .Where(x => productIds.Contains(x.ProductId))
                .Select(x => new { x.ProductId, x.UnitPrice, x.InStock })
                .ToDictionaryAsync(x => x.ProductId, x => new { x.UnitPrice, x.InStock });

            var discounts = await _discountContext.CustomerDiscounts
                .Where(x => productIds.Contains(x.ProductId)
                            && x.StartDate < DateTime.Now
                            && x.EndDate > DateTime.Now)
                .Select(x => new { x.ProductId, x.DiscountRate, x.EndDate })
                .ToDictionaryAsync(x => x.ProductId, x => new { x.DiscountRate, x.EndDate });

            foreach (var product in products)
            {
                if (inventory.TryGetValue(product.Id, out var inv))
                {
                    product.Price = inv.UnitPrice.ToMoney().ToPersianNumber();
                    product.IsInStock = inv.InStock;

                    if (discounts.TryGetValue(product.Id, out var discount) && discount.DiscountRate > 0)
                    {
                        int rate = discount.DiscountRate;
                        product.DiscountRate = rate;
                        product.DiscountExpireDate = discount.EndDate.ToFarsi().ToPersianNumber();
                        product.HasDiscount = true;

                        var discountAmount = Math.Round((inv.UnitPrice * rate) / 100);
                        product.PriceWithDiscount = (inv.UnitPrice - discountAmount).ToMoney().ToPersianNumber();
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
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                },
                Categories = categories
            };
        }

        public async Task<bool> Exists(long productId)
        {
            return await _shopContext.Products.AnyAsync(
                x => x.Id == productId);
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