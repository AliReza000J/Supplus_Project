using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.DTOs;
using _01_PharmaPlusQuery.Contracts.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PharmaStore.WebApi.Controllers.Shop
{
    /// <summary>
    /// Manages operations related to product brands.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IProductQuery _productQuery;

        /// <summary>
        /// Initializes a new instance of the BrandController.
        /// </summary>
        public BrandController(IProductQuery productQuery)
        {
            _productQuery = productQuery;
        }

        /// <summary>
        /// Searches for products by their brand name.
        /// </summary>
        /// <remarks>
        /// This endpoint allows anonymous users to retrieve a paginated list of products that belong to a specific brand.
        /// If no products are found for the given brand, a successful response is still returned, but with an empty list.
        /// </remarks>
        /// <param name="brandName">The name of the brand to search for.</param>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="pageSize">The number of items to display per page (default is 10).</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. Returns the search results containing the paginated list of products.</response>
        /// <response code="400">Bad Request. The brand name parameter is null, empty, or whitespace.</response>
        [AllowAnonymous]
        [HttpGet("{brandName}")]
        [ProducesResponseType(typeof(ApiResponse<SearchByBrandResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SearchByBrandResult>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<SearchByBrandResult>>> SearchByBrand(
            string brandName, int page = 1, int pageSize = 10, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(brandName))
                return BadRequest(ApiResponse<SearchByBrandResult>.Fail("عبارت برند نمی‌تواند خالی باشد."));

            var result = await _productQuery.GetProductsByBrand(brandName, page, pageSize, cancellation);

            if (result == null || result.Products.Items.Count == 0)
                return Ok(ApiResponse<SearchByBrandResult>.Ok(
                    result, $"هیچ محصولی برای برند «{brandName}» یافت نشد."));

            return Ok(ApiResponse<SearchByBrandResult>.Ok(
                result,
                $"جستجوی محصولات برند {brandName} با موفقیت انجام شد."
            ));
        }
    }
}
