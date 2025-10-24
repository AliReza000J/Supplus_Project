using _01_PharmaPlusQuery.Contracts.Product;
using Microsoft.AspNetCore.Mvc;

namespace ServiceHost.Controllers.Shop
{
    [Route("shop/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductQuery _productQuery;
        private readonly ILogger<ProductController> _logger;


        public ProductController(IProductQuery productQuery, ILogger<ProductController> logger)
        {
            _productQuery = productQuery;
            _logger = logger;
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