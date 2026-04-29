using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.ProductCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PharmaStore.WebApi.Controllers.Shop
{
    /// <summary>
    /// Endpoints for retrieving product categories. 
    /// Typically used by the public storefront to build navigation menus and display category pages.
    /// </summary>
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
        /// Retrieves a complete list of all product categories.
        /// </summary>
        /// <remarks>
        /// This endpoint fetches the entire category tree (full menu). 
        /// It is publicly accessible and does not require authentication.
        /// </remarks>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>A list of product categories.</returns>
        /// <response code="200">Returns the list of product categories successfully.</response>
        /// <response code="404">If no product categories are found in the system.</response>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ProductCategoryQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductCategoryQueryModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ProductCategoryQueryModel>>>> GetAllCategories(CancellationToken cancellation) //full menu
        {
            var result = await _productCategoryQuery.GetAll(cancellation);

            if (result == null || !result.Any())
                return NotFound(ApiResponse<ProductCategoryQueryModel>
                    .Fail(message: "هیچ دسته‌بندی محصولی یافت نشد."));

            return Ok(ApiResponse<List<ProductCategoryQueryModel>>.Ok(result));
        }

        /// <summary>
        /// Retrieves a specific product category by its full slug path.
        /// </summary>
        /// <remarks>
        /// Uses a catch-all route parameter to support deeply nested category structures (e.g., "electronics/laptops/gaming").
        /// </remarks>
        /// <param name="path">The full, URL-friendly path (slug) of the requested category.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>The details of the requested product category.</returns>
        /// <response code="200">Returns the matching product category.</response>
        /// <response code="400">If the provided path is null, empty, or consists only of white-space characters.</response>
        /// <response code="404">If no product category matches the specified path.</response>
        [AllowAnonymous]
        [HttpGet("{*path}")]
        [ProducesResponseType(typeof(ApiResponse<ProductCategoryQueryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProductCategoryQueryModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ProductCategoryQueryModel>), StatusCodes.Status404NotFound)]
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