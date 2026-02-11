using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.ProductCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServiceHost.Controllers.Shop
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryQuery _productCategoryQuery;
        private readonly ILogger<ProductCategoryController> _logger;

        public ProductCategoryController(
            IProductCategoryQuery productCategoryQuery,
            ILogger<ProductCategoryController> logger)
        {
            _productCategoryQuery = productCategoryQuery;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all product categories (full menu).
        /// </summary>
        /// <remarks>
        /// This endpoint returns a complete list of all available product categories.
        /// If no categories exist, a 404 response is returned.
        /// </remarks>
        /// <response code="200">Returns the list of product categories.</response>
        /// <response code="404">No product categories were found.</response>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProductCategoryQueryModel>>>> GetAllCategories(CancellationToken cancellation) //full menu
        {
            var result = await _productCategoryQuery.GetAll(cancellation);

            if (result == null || !result.Any())
                return NotFound(ApiResponse<ProductCategoryQueryModel>
                    .Fail(message: "هیچ دسته‌بندی محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductCategoryQueryModel>>.Ok(result));
        }

        /// <summary>
        /// Retrieves a product category using its full hierarchical slug path.
        /// </summary>
        /// <param name="path">The hierarchical slug path representing the category.</param>
        /// <param name="cancellation"></param>
        /// <remarks>
        /// This endpoint supports nested slugs using a catch-all route.
        /// For example: /shop/ProductCategory/health/vitamins/minerals
        /// It resolves the full path from the URL and returns the matching category.
        /// </remarks>
        /// <response code="200">Returns the matched product category.</response>
        /// <response code="400">Invalid or empty path provided.</response>
        /// <response code="404">No category found for the given path.</response>
        [AllowAnonymous]
        [HttpGet("{*path}")]
        public async Task<ActionResult<ApiResponse<ProductCategoryQueryModel>>> GetCategoryByPath(string path, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(ApiResponse<ProductCategoryQueryModel>
                    .Fail(message: "مسیر نامعتبر."));

            var productCategories = await _productCategoryQuery.GetCategoryByFullSlug(path, cancellation);

            if (productCategories == null)
                return NotFound(ApiResponse<ProductCategoryQueryModel>
                    .Fail(message: $"دسته‌بندی محصول {path} یافت نشد."));

            return Ok(ApiResponse<ProductCategoryQueryModel>.Ok(data: productCategories));
        }
    }
}