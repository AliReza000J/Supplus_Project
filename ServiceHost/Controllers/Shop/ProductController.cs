using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Product;
using _01_PharmaPlusQuery.Contracts.ProductCategory;
using AccountManagement.Application.Contracts.Account;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagement.Application.Contracts.Product;
using System.Security.Claims;

namespace ServiceHost.Controllers.Shop
{
    [Route("shop/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductQuery _productQuery;
        private readonly IProductApplication _productApplication;
        private readonly IProductCategoryQuery _productCategoryQuery;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductQuery productQuery,
            ILogger<ProductController> logger, IProductCategoryQuery productCategoryQuery, IProductApplication productApplication)
        {
            _productQuery = productQuery;
            _logger = logger;
            _productCategoryQuery = productCategoryQuery;
            _productApplication = productApplication;
        }

        /// <summary>
        /// Retrieves product details using a full hierarchical URL path.
        /// </summary>
        /// <param name="fullPath">The full slug path including category path + product slug.</param>
        /// <remarks>
        /// This endpoint supports nested category paths.  
        /// Example: /shop/product/health/vitamins/magnesium/magnesium-500mg  
        /// The last segment is considered the product slug, and the previous segments represent the category hierarchy.
        /// The method validates that the product exists and it belongs to the provided category path.
        /// </remarks>
        /// <response code="200">Returns product details.</response>
        /// <response code="400">Invalid or incomplete product path.</response>
        /// <response code="404">Product not found or does not match the category path.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [AllowAnonymous]
        [HttpGet("{*fullPath}")]
        public async Task<ActionResult<ApiResponse<ProductQueryModel>>> GetProductDetails(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return BadRequest(ApiResponse<ProductQueryModel>
                    .Fail(message: "مسیر محصول معتبر نیست."));

            var segments = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
                return BadRequest(ApiResponse<ProductQueryModel>
                    .Fail(message: "آدرس محصول ناقص است."));

            // Extract product slug (last part of the URL)
            var productSlug = segments.Last();

            // Extract category path (all segments except the last)
            var categoryPath = string.Join('/', segments.Take(segments.Length - 1));

            // Fetch product by slug
            var product = await _productQuery.GetProductDetails(productSlug);
            if (product == null)
                return NotFound(ApiResponse<ProductQueryModel>.Fail(message: "محصول یافت نشد."));

            // Validate category path matches the product's category
            var category = await _productCategoryQuery.GetCategoryByFullSlug(categoryPath);
            if (category == null || product.CategorySlug != category.Slug)
                return NotFound(ApiResponse<ProductQueryModel>
                    .Fail(message: "محصول با مسیر گروه محصول موردنظر مطابقت ندارد."));

            var productId = product.Id;
            if (productId <= 0)
                return NotFound(ApiResponse<ProductQueryModel>.Fail(message: "شناسه محصول معتبر نیست."));

            string cookieName = $"pv_{productId}";

            long? userId = null;
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(idClaim, out var parsedId))
                userId = parsedId;

            var usernname = User.FindFirst(ClaimTypes.Name)?.Value ?? "ANON";

            if (!Request.Cookies.ContainsKey(cookieName))
            {
                var now = DateTime.UtcNow;

                var op = await _productApplication.IncrementProductViewCountAsync(productId, userId.ToString());
                if (op.IsSucceeded)
                {
                    Response.Cookies.Append(
                    cookieName,
                    $"1ck-{usernname}_{now}",
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddHours(16),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax
                    }
                );
                }
            }

            return Ok(ApiResponse<ProductQueryModel>.Ok(data: product));
        }

        /// <summary>
        /// Adds the specified product to the user's list of favorite products.
        /// </summary>
        /// <remarks>
        /// This endpoint extracts the product information By Id, 
        /// validates the product and then adds the product 
        /// to the authenticated user's favorite list.
        /// 
        /// The user must be authenticated using a JWT Bearer token.
        /// </remarks>
        /// <param name="productId">
        /// The Id the product.
        /// </param>
        /// <response code="200">The product was successfully added to the favorites.</response>
        /// <response code="400">
        /// Returned when the product path is already marked as favorite.
        /// </response>
        /// <response code="401">Returned when the user is not authenticated.</response>
        /// <response code="404">Returned when the product does not exist.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("favorite/{productId}")]
        public async Task<ActionResult<ApiResponse<object>>> AddToFavorite(long productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(userId, out var accountId))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail($"شناسه کاربری {accountId} نامعتبر است."));

            if (!await _productQuery.Exists(productId))
                return NotFound(ApiResponse<object>.Fail("محصول یافت نشد."));

            var op = await _productApplication.AddFavoriteAsync(userId, productId);

            var result = new ApiResponse<object>
            {
                Success = op.IsSucceeded,
                Message = op.Message
            };

            if (!op.IsSucceeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Removes the specified product from the user's list of favorite products.
        /// </summary>
        /// <remarks>
        /// This endpoint validates the product and removes the product 
        /// from the authenticated user's list of favorites.
        /// 
        /// The user must be authenticated using a JWT Bearer token.
        /// </remarks>
        /// <param name="productId">
        /// The Id the product.
        /// </param>
        /// <response code="200">The product was successfully removed from favorites.</response>
        /// <response code="400">
        /// Returned when the product is not found in the user's favorites.
        /// </response>
        /// <response code="401">Returned when the user is not authenticated.</response>
        /// <response code="404">Returned when the product does not exist.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("favorite/{productId}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteFavorite(long productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(userId, out var accountId))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail($"شناسه کاربری {accountId} نامعتبر است."));

            if (!await _productQuery.Exists(productId))
                return NotFound(ApiResponse<object>.Fail("محصول یافت نشد."));

            var op = await _productApplication.DeleteFavoriteAsync(userId, productId);

            var result = new ApiResponse<object>
            {
                Success = op.IsSucceeded,
                Message = op.Message
            };

            if (!op.IsSucceeded)
                return BadRequest(result);

            return Ok(result);
        }


        /// <summary>
        /// Retrieves the latest product arrivals.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of recently added products.  
        /// If no products exist, a 404 response is returned.
        /// </remarks>
        /// <response code="200">Returns the latest products.</response>
        /// <response code="404">No products were found.</response>
        [AllowAnonymous]
        [HttpGet("latest-arrivals")]
        public async Task<ActionResult<ApiResponse<List<ProductQueryModel>>>> GetLatestArrivals()
        {
            var products = await _productQuery.GetLatestArrivals();

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductQueryModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductQueryModel>>.Ok(data: products));
        }

        /// <summary>
        /// Searches for products based on a text value with pagination.
        /// </summary>
        /// <param name="value">Search term provided by the user.</param>
        /// <param name="page">Page number for pagination.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <remarks>
        /// Performs a full-text search within product titles, descriptions, and related searchable fields.  
        /// Returns a paged response with the list of matched products.
        /// </remarks>
        /// <response code="200">Returns search results (may be empty).</response>
        /// <response code="400">Search value is empty.</response>
        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<PagedResult<ProductQueryModel>>>> Search(
            string value, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(value))
                return BadRequest(ApiResponse<PagedResult<ProductQueryModel>>
                    .Fail("عبارت جستجو نمی‌تواند خالی باشد."));

            var result = await _productQuery.Search(value, page, pageSize);

            return Ok(ApiResponse<PagedResult<ProductQueryModel>>.Ok(
                result,
                result.Items.Any()
                    ? "جستجو با موفقیت انجام شد."
                    : $"هیچ محصولی برای عبارت «{value}» یافت نشد."
            ));
        }

        /// <summary>
        /// Gets a random list of available (in-stock) products.
        /// </summary>
        /// <remarks>
        /// Retrieves a random selection of products from the catalog.  
        /// Only products that are currently in stock are eligible for selection.  
        /// The number of products returned is defined by the <c>take</c> parameter (default: 10).  
        /// This endpoint is intended for showing "You may also like" or "Recommended for you" items.
        /// </remarks>
        /// <param name="take">Number of random products to return.</param>
        /// <response code="200">Returns a random list of available products (may be fewer than requested).</response>
        /// <response code="404">No in-stock products were found.</response>
        [AllowAnonymous]
        [HttpGet("random-recommendations")]
        public async Task<ActionResult<ApiResponse<List<ProductQueryModel>>>> RandomRecommendations(int take = 10)
        {
            var products = await _productQuery.GetRandomRecommendations(take);

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductQueryModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductQueryModel>>.Ok(data: products));
        }

        /// <summary>
        /// Gets the most popular products based on average user rating.
        /// </summary>
        /// <remarks>
        /// Calculates the average rating for each product using confirmed and non-canceled comments  
        /// with a valid rating value (1 to 5).  
        /// Only products that are currently in stock are included.  
        /// Returns the top rated products ordered by highest average rating and then by rating count.  
        /// Useful for displaying "Top Rated Products" or "Most Popular Items".
        /// </remarks>
        /// <param name="take">Number of top rated products to return (default: 10).</param>
        /// <response code="200">Returns the list of top rated products (may be fewer than requested).</response>
        /// <response code="404">No rated or in-stock products were found.</response>
        [AllowAnonymous]
        [HttpGet("top-rated")]
        public async Task<ActionResult<ApiResponse<List<ProductQueryModel>>>> TopRatedProducts(int take = 10)
        {
            var products = await _productQuery.GetTopRatedProducts(take);

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductQueryModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductQueryModel>>.Ok(data: products));
        }

        /// <summary>
        /// Gets the most viewed products based on view counts.
        /// </summary>
        /// <remarks>
        /// Only products that are currently in stock are included.  
        /// Returns the most viewed products ordered by highest view counts and then by average rating and rating count.  
        /// Useful for displaying "Most Viewed Products" or "Most Visited Products".
        /// </remarks>
        /// <param name="take">Number of top rated products to return (default: 10).</param>
        /// <response code="200">Returns the list of top rated products (may be fewer than requested).</response>
        /// <response code="404">No rated or in-stock products were found.</response>
        [AllowAnonymous]
        [HttpGet("most-viewed")]
        public async Task<ActionResult<ApiResponse<List<ProductQueryModel>>>> MostViewedProducts(int take = 10)
        {
            var products = await _productQuery.GetMostViewedProducts(take);

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductQueryModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductQueryModel>>.Ok(data: products));
        }
    }
}
