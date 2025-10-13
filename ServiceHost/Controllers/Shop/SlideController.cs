using _01_PharmaPlusQuery.Contracts.Slide;
using Microsoft.AspNetCore.Mvc;

namespace ServiceHost.Controllers.Shop
{
    [Route("shop/[controller]")]
    [ApiController]
    public class SlideController : ControllerBase
    {
        private readonly ISlideQuery _slideQuery;
        private readonly ILogger<SlideController> _logger;

        public SlideController(ISlideQuery slideQuery, ILogger<SlideController> logger)
        {
            _slideQuery = slideQuery;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetSlides()
        {
            try
            {
                var slides = await _slideQuery.GetSlides();

                if (slides == null || !slides.Any())
                    return NotFound("هیچ اسلایدی برای نمایش یافت نشد.");

                return Ok(slides);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "ورودی نامعتبر در دریافت اسلایدها");
                return BadRequest(new
                {
                    error = "خطای ورودی",
                    message = "داده‌های ارسال‌شده معتبر نیستند."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "خطای منطقی در پردازش اسلایدها");
                return StatusCode(StatusCodes.Status409Conflict, new
                {
                    error = "تداخل در پردازش",
                    message = "عملیات مورد نظر با مشکل مواجه شد."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطای غیرمنتظره در دریافت اسلایدها");
                return Problem(
                    title: "خطای سرور",
                    detail: "در هنگام پردازش درخواست خطایی رخ داده است. لطفاً مجدداً تلاش کنید.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}