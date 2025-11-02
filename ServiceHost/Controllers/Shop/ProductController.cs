using _01_PharmaPlusQuery.Contracts.Product;
using _01_PharmaPlusQuery.Contracts.ProductCategory;
using _01_PharmaPlusQuery.Query;
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


        public ProductController(IProductQuery productQuery, ILogger<ProductController> logger, IProductCategoryQuery productCategoryQuery)
        {
            _productQuery = productQuery;
            _logger = logger;
            _productCategoryQuery = productCategoryQuery;
        }

        [HttpGet("{*fullPath}")]
        public async Task<IActionResult> GetProductDetails(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return BadRequest("مسیر محصول معتبر نیست.");

            try
            {
                var segments = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length < 2)
                    return BadRequest("آدرس محصول ناقص است.");

                var productSlug = segments.Last();
                var categoryPath = string.Join('/', segments.Take(segments.Length - 1));

                var product = await _productQuery.GetProductDetails(productSlug);
                if (product == null)
                    return NotFound("محصول یافت نشد.");

                var category = await _productCategoryQuery.GetCategoryByFullSlug(categoryPath);
                if (category == null || product.CategorySlug != category.Slug)
                    return NotFound("محصول با مسیر گروه محصول موردنظر مطابقت ندارد.");

                return Ok(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Product details failed: {ex.Message}");
                return StatusCode(500, "خطای داخلی سرور هنگام دریافت جزئیات محصول.");
            }
        }

        [HttpGet("latest-arrivals")]
        public async Task<IActionResult> GetLatestArrivals()
        {
            try
            {
                var products = await _productQuery.GetLatestArrivals();

                if (products == null || !products.Any())
                    return NotFound("هیچ محصولی یافت نشد.");

                return Ok(products);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "ورودی نامعتبر در دریافت محصولات");
                return BadRequest(new
                {
                    error = "خطای ورودی",
                    message = "داده‌های ارسال‌شده معتبر نیستند."
                });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return BadRequest(new
                    {
                        error = "ورودی نامعتبر",
                        message = "عبارت جستجو نمی‌تواند خالی باشد."
                    });
                }

                var products = await _productQuery.Search(value);

                if (products == null || !products.Any())
                    return NotFound(new
                    {
                        message = $"هیچ محصولی با عبارت «{value}» یافت نشد."
                    });

                return Ok(products);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "ورودی نامعتبر برای جستجوی محصولات: {Value}", value);
                return BadRequest(new
                {
                    error = "خطای ورودی",
                    message = "داده‌های ارسال‌شده معتبر نیستند."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "خطای منطقی در جستجوی محصولات: {Value}", value);
                return StatusCode(StatusCodes.Status409Conflict, new
                {
                    error = "تداخل در پردازش",
                    message = "عملیات جستجو با خطا مواجه شد."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطای غیرمنتظره در جستجوی محصولات: {Value}", value);
                return Problem(
                    title: "خطای سرور",
                    detail: "در هنگام پردازش درخواست جستجو خطایی رخ داده است. لطفاً مجدداً تلاش کنید.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}