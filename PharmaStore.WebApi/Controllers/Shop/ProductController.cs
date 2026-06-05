using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Product;
using _01_PharmaPlusQuery.Contracts.ProductCategory;
using AccountManagement.Application.Contracts.Account.ApplicationDTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ShopManagement.Application.Contracts.Product;
using System.Security.Claims;

namespace PharmaStore.WebApi.Controllers.Shop
{
    /// <summary>
    /// Endpoints for managing and retrieving products, including details, searches, recommendations, and user favorites.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly IProductQuery _productQuery;
        private readonly IProductApplication _productApplication;
        private readonly IProductCategoryQuery _productCategoryQuery;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductQuery productQuery,
            ILogger<ProductController> logger, IProductCategoryQuery productCategoryQuery, IProductApplication productApplication, IMemoryCache cache)
        {
            _productQuery = productQuery;
            _logger = logger;
            _productCategoryQuery = productCategoryQuery;
            _productApplication = productApplication;
            _cache = cache;
        }

        /// <summary>
        /// Retrieves detailed information about a specific product based on its full URL path.
        /// </summary>
        /// <remarks>
        /// The path must include both the category slug(s) and the product slug (e.g., "electronics/laptops/macbook-pro").
        /// This endpoint also tracks product views using cookies to prevent duplicate view counts from the same user/session within 16 hours.
        /// If an authenticated user requests this endpoint, their user ID is extracted to log authenticated views.
        /// </remarks>
        /// <param name="fullPath">The complete path including the category tree and product slug.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The detailed product information.</returns>
        /// <response code="200">Returns the product details successfully.</response>
        /// <response code="400">If the provided path is invalid or incomplete.</response>
        /// <response code="404">If the product or its matching category is not found.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [AllowAnonymous]
        [HttpGet("{*fullPath}")]
        [ProducesResponseType(typeof(ApiResponse<ProductQueryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProductQueryModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ProductQueryModel>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProductQueryModel>>> GetProductDetails(string fullPath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return BadRequest(ApiResponse<ProductQueryModel>
                    .Fail(message: "مسیر محصول معتبر نیست."));

            var segments = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
                return BadRequest(ApiResponse<ProductQueryModel>
                    .Fail(message: "آدرس محصول ناقص است."));

            long userId = 0;
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (long.TryParse(idClaim, out var parsedId))
                userId = parsedId;

            // Extract product slug (last part of the URL)
            var productSlug = segments.Last();

            // Extract category path (all segments except the last)
            var categoryPath = string.Join('/', segments.Take(segments.Length - 1));

            // Fetch product by slug
            var product = await _productQuery.GetProductDetails(productSlug, parsedId, cancellationToken);
            if (product == null)
                return NotFound(ApiResponse<ProductQueryModel>.Fail(message: "محصول یافت نشد."));

            // Validate category path matches the product's category
            var category = await _productCategoryQuery.GetCategoryByFullSlug(categoryPath, cancellationToken);
            if (category == null || product.CategorySlug != category.Slug)
                return NotFound(ApiResponse<ProductQueryModel>
                    .Fail(message: "محصول با مسیر گروه محصول موردنظر مطابقت ندارد."));

            var productId = product.Id;
            if (productId <= 0)
                return NotFound(ApiResponse<ProductQueryModel>.Fail(message: "شناسه محصول معتبر نیست."));

            var deviceId = Request.Headers["X-Device-Id"].ToString();

            string clientIdentifier;
            if (userId > 0)
            {
                clientIdentifier = $"U_{userId}";
            }
            else if (!string.IsNullOrEmpty(deviceId))
            {
                clientIdentifier = $"D_{deviceId}";
            }
            else
            {
                clientIdentifier = $"IP_{HttpContext.Connection.RemoteIpAddress}";
            }

            string cacheKey = $"pv_{productId}_{clientIdentifier}";

            if (!_cache.TryGetValue(cacheKey, out _))
            {
                var op = await _productApplication.IncreaseProductViewCountAsync(productId, userId, cancellationToken);
                if (op.IsSucceeded)
                {
                    _cache.Set(cacheKey, true, TimeSpan.FromHours(16));
                }
            }

            return Ok(ApiResponse<ProductQueryModel>.Ok(data: product));
        }

        /// <summary>
        /// Adds a specific product to the authenticated user's favorites list.
        /// </summary>
        /// <remarks>
        /// Requires a valid JWT token.
        /// </remarks>
        /// <param name="productId">The unique identifier of the product to favorite.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A response indicating success or failure of the operation.</returns>
        /// <response code="200">The product was successfully added to favorites.</response>
        /// <response code="400">If the operation fails (e.g., already in favorites).</response>
        /// <response code="401">If the user is not authenticated or the token is invalid.</response>
        /// <response code="404">If the specified product does not exist.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("favorite/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AccountViewModel>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> AddToFavorite(long productId, CancellationToken cancellationToken)
        {
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(nameIdentifier))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(nameIdentifier, out var userId))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            if (!await _productQuery.Exists(productId, cancellationToken))
                return NotFound(ApiResponse<object>.Fail("محصول یافت نشد."));

            var op = await _productApplication.AddFavoriteAsync(userId, productId, cancellationToken);

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
        /// Removes a specific product from the authenticated user's favorites list.
        /// </summary>
        /// <remarks>
        /// Requires a valid JWT token.
        /// </remarks>
        /// <param name="productId">The unique identifier of the product to remove from favorites.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A response indicating success or failure of the removal operation.</returns>
        /// <response code="200">The product was successfully removed from favorites.</response>
        /// <response code="400">If the operation fails.</response>
        /// <response code="401">If the user is not authenticated or the token is invalid.</response>
        /// <response code="404">If the specified product does not exist.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("favorite/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AccountViewModel>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteFavorite(long productId, CancellationToken cancellationToken)
        {
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(nameIdentifier))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(nameIdentifier, out var userId))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            if (!await _productQuery.Exists(productId, cancellationToken))
                return NotFound(ApiResponse<object>.Fail("محصول یافت نشد."));

            var op = await _productApplication.DeleteFavoriteAsync(userId, productId, cancellationToken);

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
        /// Retrieves a list of the most recently added products (Latest Arrivals).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of new product arrivals.</returns>
        /// <response code="200">Returns the list of latest products successfully.</response>
        /// <response code="404">If no products are found in the system.</response>
        [AllowAnonymous]
        [HttpGet("latest-arrivals")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductCardModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductCardModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ProductCardModel>>>> GetLatestArrivals(CancellationToken cancellationToken)
        {
            var products = await _productQuery.GetLatestArrivals(cancellationToken);

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductCardModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductCardModel>>.Ok(data: products));
        }

        /// <summary>
        /// Searches for products based on a specified keyword value.
        /// </summary>
        /// <remarks>
        /// Supports pagination.
        /// </remarks>
        /// <param name="request"></param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paginated list of products matching the search criteria.</returns>
        /// <response code="200">Returns the paginated search results.</response>
        /// <response code="400">If the search value is null or empty.</response>
        [AllowAnonymous]
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductCardModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductCardModel>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PagedResult<ProductCardModel>>>> Search(
        [FromQuery] ProductFilterRequest request,
        CancellationToken cancellationToken = default)
        {
            // محدودیت برای جلوگیری از لود سنگین
            if (request.PageSize > 50) request.PageSize = 50;

            var result = await _productQuery.Search(request, cancellationToken);

            return Ok(ApiResponse<PagedResult<ProductCardModel>>.Ok(
                result,
                result.Items.Any() ? "جستجو با موفقیت انجام شد." : "محصولی یافت نشد."
            ));
        }

        /// <summary>
        /// Retrieves a list of random product recommendations.
        /// </summary>
        /// <param name="take">The number of random products to retrieve (default is 10).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of randomly recommended products.</returns>
        /// <response code="200">Returns the list of recommended products.</response>
        /// <response code="404">If no products are found.</response>
        [AllowAnonymous]
        [HttpGet("random-recommendations")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductCardModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductCardModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ProductCardModel>>>> RandomRecommendations(int take = 10, CancellationToken cancellationToken = default)
        {
            var products = await _productQuery.GetRandomRecommendations(take, cancellationToken);

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductCardModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductCardModel>>.Ok(data: products));
        }

        /// <summary>
        /// Retrieves a list of the top-rated products.
        /// </summary>
        /// <param name="take">The maximum number of products to return (default is 10).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of the highest-rated products.</returns>
        /// <response code="200">Returns the top-rated products successfully.</response>
        /// <response code="404">If no products are found.</response>
        [AllowAnonymous]
        [HttpGet("top-rated")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductCardModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductCardModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ProductCardModel>>>> TopRatedProducts(int take = 10, CancellationToken cancellationToken = default)
        {
            var products = await _productQuery.GetTopRatedProducts(take, cancellationToken);

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductCardModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductCardModel>>.Ok(data: products));
        }

        /// <summary>
        /// Retrieves a list of the most viewed products across the store.
        /// </summary>
        /// <param name="take">The maximum number of products to return (default is 10).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of the most viewed products.</returns>
        /// <response code="200">Returns the most viewed products successfully.</response>
        /// <response code="404">If no products are found.</response>
        [AllowAnonymous]
        [HttpGet("most-viewed")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductCardModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductCardModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ProductCardModel>>>> MostViewedProducts(int take = 10, CancellationToken cancellationToken = default)
        {
            var products = await _productQuery.GetMostViewedProducts(take, cancellationToken);

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductCardModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductCardModel>>.Ok(data: products));
        }
    }
}
