using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Product;
using _01_PharmaPlusQuery.Contracts.ProductCategory;
using AccountManagement.Application.Contracts.Account.ApplicationDTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaShared.Web.Filters;
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

            string cookieName = $"pv_{productId}";

            var usernname = User.FindFirst(ClaimTypes.Name)?.Value ?? "ANON";

            if (!Request.Cookies.ContainsKey(cookieName))
            {
                var now = DateTime.UtcNow;

                var op = await _productApplication.IncreaseProductViewCountAsync(productId, userId, cancellationToken);
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
        /// Adds a specific product to the authenticated user's favorites list.
        /// </summary>
        /// <remarks>
        /// Requires a valid JWT token. Rate limited to 3 requests per 60 seconds to prevent abuse.
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
        [RateLimit(Limit = 3, Seconds = 60)]
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
        /// Requires a valid JWT token. Rate limited to 3 requests per 60 seconds.
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
        [RateLimit(Limit = 3, Seconds = 60)]
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
        [ProducesResponseType(typeof(ApiResponse<List<ProductQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductQueryModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ProductQueryModel>>>> GetLatestArrivals(CancellationToken cancellationToken)
        {
            var products = await _productQuery.GetLatestArrivals(cancellationToken);

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductQueryModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductQueryModel>>.Ok(data: products));
        }

        /// <summary>
        /// Searches for products based on a specified keyword value.
        /// </summary>
        /// <remarks>
        /// Rate limited to 30 requests per 60 seconds. Supports pagination.
        /// </remarks>
        /// <param name="value">The keyword or phrase to search for.</param>
        /// <param name="page">The current page number (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paginated list of products matching the search criteria.</returns>
        /// <response code="200">Returns the paginated search results.</response>
        /// <response code="400">If the search value is null or empty.</response>
        [AllowAnonymous]
        [HttpGet("search")]
        [RateLimit(Limit = 30, Seconds = 60)]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductQueryModel>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PagedResult<ProductQueryModel>>>> Search(
            string value, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(value))
                return BadRequest(ApiResponse<PagedResult<ProductQueryModel>>
                    .Fail("عبارت جستجو نمی‌تواند خالی باشد."));

            var result = await _productQuery.Search(value, page, pageSize, cancellationToken);

            return Ok(ApiResponse<PagedResult<ProductQueryModel>>.Ok(
                result,
                result.Items.Any()
                    ? "جستجو با موفقیت انجام شد."
                    : $"هیچ محصولی برای عبارت «{value}» یافت نشد."
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
        [ProducesResponseType(typeof(ApiResponse<List<ProductQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductQueryModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ProductQueryModel>>>> RandomRecommendations(int take = 10, CancellationToken cancellationToken = default)
        {
            var products = await _productQuery.GetRandomRecommendations(take, cancellationToken);

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductQueryModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductQueryModel>>.Ok(data: products));
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
        [ProducesResponseType(typeof(ApiResponse<List<ProductQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductQueryModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ProductQueryModel>>>> TopRatedProducts(int take = 10, CancellationToken cancellationToken = default)
        {
            var products = await _productQuery.GetTopRatedProducts(take, cancellationToken);

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductQueryModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductQueryModel>>.Ok(data: products));
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
        [ProducesResponseType(typeof(ApiResponse<List<ProductQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductQueryModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ProductQueryModel>>>> MostViewedProducts(int take = 10, CancellationToken cancellationToken = default)
        {
            var products = await _productQuery.GetMostViewedProducts(take, cancellationToken);

            if (products == null || !products.Any())
                return NotFound(ApiResponse<List<ProductQueryModel>>
                    .Fail(message: "هیچ محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductQueryModel>>.Ok(data: products));
        }
    }
}
