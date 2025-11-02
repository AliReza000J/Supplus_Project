using _01_PharmaPlusQuery.Contracts.Article;
using _01_PharmaPlusQuery.Contracts.ArticleCategory;
using Microsoft.AspNetCore.Mvc;

namespace ServiceHost.Controllers.Blog
{
    [Route("blog/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly ILogger<ArticleController> _logger;
        private readonly IArticleQuery _articleQuery;
        private readonly IArticleCategoryQuery _articleCategoryQuery;

        public ArticleController(IArticleQuery articleQuery, IArticleCategoryQuery articleCategoryQuery, ILogger<ArticleController> logger)
        {
            _articleQuery = articleQuery;
            _articleCategoryQuery = articleCategoryQuery;
            _logger = logger;
        }

        [HttpGet("{categorySlug}/{articleSlug}")]
        public async Task<IActionResult> GetArticleDetails(string articleSlug, string categorySlug)
        {
            if (string.IsNullOrWhiteSpace(articleSlug))
                return BadRequest("مسیر مقاله معتبر نیست.");

            try
            {
                var article = await _articleQuery.GetArticleDetails(articleSlug);
                if (article == null)
                    return NotFound("مقاله یافت نشد.");

                var category = await _articleCategoryQuery.GetArticleCategory(categorySlug);
                if (category == null || article.CategorySlug != category.Slug)
                    return NotFound("مقاله با مسیر گروه مقاله موردنظر مطابقت ندارد.");

                return Ok(article);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Article details failed: {ex.Message}");
                return StatusCode(500, "خطای داخلی سرور هنگام دریافت جزئیات مقاله.");
            }
        }

        [HttpGet("latest-arrivals")]
        public async Task<IActionResult> GetLatestArrivals()
        {
            try
            {
                var articles = await _articleQuery.LatestArticles();

                if (articles == null || !articles.Any())
                    return NotFound("هیچ مقاله‌ای یافت نشد.");

                return Ok(articles);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "ورودی نامعتبر در دریافت مقالات");
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

                var articles = await _articleQuery.Search(value);

                if (articles == null || !articles.Any())
                    return NotFound(new
                    {
                        message = $"هیچ مقاله‌ای با عبارت «{value}» یافت نشد."
                    });

                return Ok(articles);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "ورودی نامعتبر برای جستجوی مقالات: {Value}", value);
                return BadRequest(new
                {
                    error = "خطای ورودی",
                    message = "داده‌های ارسال‌شده معتبر نیستند."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "خطای منطقی در جستجوی مقالات: {Value}", value);
                return StatusCode(StatusCodes.Status409Conflict, new
                {
                    error = "تداخل در پردازش",
                    message = "عملیات جستجو با خطا مواجه شد."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطای غیرمنتظره در جستجوی مقالات: {Value}", value);
                return Problem(
                    title: "خطای سرور",
                    detail: "در هنگام پردازش درخواست جستجو خطایی رخ داده است. لطفاً مجدداً تلاش کنید.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
