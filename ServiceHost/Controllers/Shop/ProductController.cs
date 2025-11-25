using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Product;
using _01_PharmaPlusQuery.Contracts.ProductCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServiceHost.Controllers.Shop
{
    [Route("shop/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductQuery _productQuery;
        private readonly IProductCategoryQuery _productCategoryQuery;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductQuery productQuery,
            ILogger<ProductController> logger, IProductCategoryQuery productCategoryQuery)
        {
            _productQuery = productQuery;
            _logger = logger;
            _productCategoryQuery = productCategoryQuery;
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

            return Ok(ApiResponse<ProductQueryModel>.Ok(data: product));
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
    }
}
