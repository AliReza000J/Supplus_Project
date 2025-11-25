using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Article;
using _01_PharmaPlusQuery.Contracts.ArticleCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServiceHost.Controllers.Blog
{
    /// <summary>
    /// Provides access to article-related public endpoints.
    /// </summary>
    /// <remarks>
    /// This controller exposes endpoints for viewing article details,
    /// retrieving the latest published articles, and searching articles.
    /// </remarks>
    [Route("blog/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly ILogger<ArticleController> _logger;
        private readonly IArticleQuery _articleQuery;
        private readonly IArticleCategoryQuery _articleCategoryQuery;

        /// <summary>
        /// Initializes the <see cref="ArticleController"/>.
        /// </summary>
        /// <param name="articleQuery">Query service for retrieving article data.</param>
        /// <param name="articleCategoryQuery">Query service for article categories.</param>
        /// <param name="logger">Logger used for diagnostic and error tracking.</param>
        public ArticleController(IArticleQuery articleQuery, IArticleCategoryQuery articleCategoryQuery, ILogger<ArticleController> logger)
        {
            _articleQuery = articleQuery;
            _articleCategoryQuery = articleCategoryQuery;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves article details using both category slug and article slug.
        /// </summary>
        /// <param name="articleSlug">The slug of the article.</param>
        /// <param name="categorySlug">The slug of the category to which the article belongs.</param>
        /// <remarks>
        /// This endpoint ensures the article exists and also verifies that
        /// the article belongs to the category specified in the URL.
        /// </remarks>
        /// <returns>
        /// The article details wrapped in <see cref="ApiResponse{T}"/>.
        /// </returns>
        /// <response code="200">Article found and returned successfully.</response>
        /// <response code="400">Invalid article slug.</response>
        /// <response code="404">Article not found or does not match the category.</response>
        [AllowAnonymous]
        [HttpGet("{categorySlug}/{articleSlug}")]
        public async Task<ActionResult<ApiResponse<ArticleQueryModel>>> GetArticleDetails(string articleSlug, string categorySlug)
        {
            if (string.IsNullOrWhiteSpace(articleSlug))
                return BadRequest(ApiResponse<ArticleQueryModel>.Fail(message: "مسیر مقاله معتبر نیست."));

            var article = await _articleQuery.GetArticleDetails(articleSlug);
            if (article == null)
                return NotFound(ApiResponse<ArticleQueryModel>.Fail(message: "مقاله یافت نشد."));

            var category = await _articleCategoryQuery.GetArticleCategory(categorySlug);
            if (category == null || article.CategorySlug != category.Slug)
                return NotFound(ApiResponse<ArticleQueryModel>.Fail(message: "مقاله با مسیر گروه مقاله موردنظر مطابقت ندارد."));

            return Ok(ApiResponse<ArticleQueryModel>.Ok(data: article));
        }

        /// <summary>
        /// Retrieves the list of most recently added articles.
        /// </summary>
        /// <remarks>
        /// Returns the newest published articles ordered by publish date descending.
        /// </remarks>
        /// <returns>
        /// A list of the latest articles wrapped in <see cref="ApiResponse{T}"/>.
        /// </returns>
        /// <response code="200">Articles returned successfully.</response>
        /// <response code="404">No articles found.</response>
        [AllowAnonymous]
        [HttpGet("latest-arrivals")]
        public async Task<ActionResult<ApiResponse<List<ArticleQueryModel>>>> GetLatestArrivals()
        {
            var articles = await _articleQuery.LatestArticles();

            if (articles == null || !articles.Any())
                return NotFound(ApiResponse<List<ArticleQueryModel>>.Fail(message: "هیچ مقاله‌ای یافت نشد."));

            return Ok(ApiResponse<List<ArticleQueryModel>>.Ok(data: articles));
        }

        /// <summary>
        /// Searches for articles based on a provided keyword.
        /// </summary>
        /// <param name="value">The search keyword.</param>
        /// <param name="page">Page number for pagination (default: 1).</param>
        /// <param name="pageSize">Number of items per page (default: 10).</param>
        /// <remarks>
        /// Performs a paginated search on article titles, descriptions, or body content.
        /// </remarks>
        /// <returns>
        /// A paginated result of matched articles wrapped in <see cref="ApiResponse{T}"/>.
        /// </returns>
        /// <response code="200">Search results returned successfully.</response>
        /// <response code="400">Search keyword is empty or invalid.</response>
        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<PagedResult<ArticleQueryModel>>>> Search(
            string value, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(value))
                return BadRequest(ApiResponse<PagedResult<ArticleQueryModel>>
                    .Fail("عبارت جستجو نمی‌تواند خالی باشد."));

            var result = await _articleQuery.Search(value, page, pageSize);

            return Ok(ApiResponse<PagedResult<ArticleQueryModel>>.Ok(
                result,
                result.Items.Any()
                    ? "جستجو با موفقیت انجام شد."
                    : $"هیچ مقاله‌ای برای عبارت «{value}» یافت نشد."
            ));
        }
    }
}