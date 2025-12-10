using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.DTOs;
using _01_PharmaPlusQuery.Contracts.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServiceHost.Controllers.Shop
{
    [Route("shop/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IProductQuery _productQuery;

        public BrandController(IProductQuery productQuery)
        {
            _productQuery = productQuery;
        }

        [AllowAnonymous]
        [HttpGet("{brandName}")]
        public async Task<ActionResult<ApiResponse<SearchByBrandResult>>> SearchByBrand(
    string brandName, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(brandName))
                return BadRequest(ApiResponse<PagedResult<ProductQueryModel>>.Fail("عبارت برند نمی‌تواند خالی باشد."));

            var result = await _productQuery.GetProductsByBrand(brandName, page, pageSize);

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
