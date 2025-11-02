using _01_PharmaPlusQuery.Contracts.ArticleCategory;
using Microsoft.AspNetCore.Mvc;

namespace ServiceHost.Controllers.Blog
{
    [Route("blog/[controller]")]
    [ApiController]
    public class ArticleCategoryController : ControllerBase
    {
        private readonly IArticleCategoryQuery _articleCategoryQuery;
        private readonly ILogger<ArticleCategoryController> _logger;

        public ArticleCategoryController(ILogger<ArticleCategoryController> logger, IArticleCategoryQuery articleCategoryQuery)
        {
            _logger = logger;
            _articleCategoryQuery = articleCategoryQuery;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var result = await _articleCategoryQuery.GetArticleCategories();

                if (result == null || !result.Any())
                    return NotFound("هیچ دسته‌بندی برای مقالات یافت نشد.");

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "ورودی نامعتبر در دریافت دسته‌بندی‌ها");
                return BadRequest(new
                {
                    error = "خطای ورودی",
                    message = "داده‌های ارسال‌شده معتبر نیستند."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "خطای منطقی در دریافت دسته‌بندی‌ها");
                return StatusCode(StatusCodes.Status409Conflict, new
                {
                    error = "تداخل در پردازش",
                    message = "عملیات مورد نظر با مشکل مواجه شد."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطای غیرمنتظره در دریافت دسته‌بندی‌ها");
                return Problem(
                    title: "خطای سرور",
                    detail: "در هنگام پردازش درخواست خطایی رخ داده است. لطفاً مجدداً تلاش کنید.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest("مسیر نامعتبر.");

            try
            {
                var articleCategories = await _articleCategoryQuery.GetArticleCategory(slug);

                if (articleCategories == null)
                    return NotFound($"دسته‌بندی مقاله {slug} یافت نشد.");

                return Ok(articleCategories);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "ورودی نامعتبر در دریافت دسته‌بندی‌ها");
                return BadRequest(new
                {
                    error = "خطای ورودی",
                    message = "داده‌های ارسال‌شده معتبر نیستند."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "خطای منطقی در دریافت دسته‌بندی‌ها");
                return StatusCode(StatusCodes.Status409Conflict, new
                {
                    error = "تداخل در پردازش",
                    message = "عملیات مورد نظر با مشکل مواجه شد."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطای غیرمنتظره در دریافت دسته‌بندی‌ها");
                return Problem(
                    title: "خطای سرور",
                    detail: "در هنگام پردازش درخواست خطایی رخ داده است. لطفاً مجدداً تلاش کنید.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
