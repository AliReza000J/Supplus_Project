using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Article;
using _01_PharmaPlusQuery.Contracts.ArticleCategory;
using BlogManagement.Application.Contracts.Article;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        private readonly IArticleApplication _articleApplication;

        /// <summary>
        /// Initializes the <see cref="ArticleController"/>.
        /// </summary>
        /// <param name="articleQuery">Query service for retrieving article data.</param>
        /// <param name="articleCategoryQuery">Query service for article categories.</param>
        /// <param name="logger">Logger used for diagnostic and error tracking.</param>
        public ArticleController(IArticleQuery articleQuery, IArticleCategoryQuery articleCategoryQuery, ILogger<ArticleController> logger, IArticleApplication articleApplication)
        {
            _articleQuery = articleQuery;
            _articleCategoryQuery = articleCategoryQuery;
            _logger = logger;
            _articleApplication = articleApplication;
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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


            var articleId = await _articleQuery.GetArticleIdBySlug(articleSlug);
            if (articleId <= 0)
                return NotFound(ApiResponse<ArticleQueryModel>.Fail(message: "شناسه مقاله معتبر نیست."));
            
            string cookieName = $"av_{articleId}";

            long? userId = null;
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(idClaim, out var parsedId))
                userId = parsedId;

            var usernname = User.FindFirst(ClaimTypes.Name)?.Value ?? "ANON";

            if (!Request.Cookies.ContainsKey(cookieName))
            {
                var now = DateTime.UtcNow;

                var op = await _articleApplication.IncrementArticleViewCountAsync(articleId, userId.ToString());
                if (op.IsSucceeded)
                {
                    Response.Cookies.Append(
                    cookieName,
                    $"1ck-{usernname}_{now}",
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddHours(16),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax
                    }
                );
                }

            }

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
            var articles = await _articleQuery.GetLatestArticles();

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

        /// <summary>
        /// Gets the most popular articles based on average user rating.
        /// </summary>
        /// <remarks>
        /// Calculates the average rating for each article using confirmed and non-canceled comments  
        /// with a valid rating value (1 to 5).  
        /// Only articles that are currently published are included.  
        /// Returns the top rated articles ordered by highest average rating and then by rating count.  
        /// Useful for displaying "Top Rated articles" or "Most Popular Items".
        /// </remarks>
        /// <param name="take">Number of top rated articles to return (default: 10).</param>
        /// <response code="200">Returns the list of top rated article (may be fewer than requested).</response>
        /// <response code="404">No rated or in-stock article were found.</response>
        [AllowAnonymous]
        [HttpGet("top-rated")]
        public async Task<ActionResult<ApiResponse<List<ArticleQueryModel>>>> TopRatedArticles(int take = 10)
        {
            var articles = await _articleQuery.GetTopRatedArticles(take);

            if (articles == null || !articles.Any())
                return NotFound(ApiResponse<List<ArticleQueryModel>>
                    .Fail(message: "هیچ مقاله‌ای یافت نشد."));

            return Ok(ApiResponse<List<ArticleQueryModel>>.Ok(data: articles));
        }

        /// <summary>
        /// Gets the most viewed articles based on view counts.
        /// </summary>
        /// <remarks>
        /// Only articles that are currently in stock are included.  
        /// Returns the most viewed articles ordered by highest view counts and then by average rating and rating count.  
        /// Useful for displaying "Most Viewed articles" or "Most Visited articles".
        /// </remarks>
        /// <param name="take">Number of top rated articles to return (default: 10).</param>
        /// <response code="200">Returns the list of top rated articles (may be fewer than requested).</response>
        /// <response code="404">No rated articles were found.</response>
        [AllowAnonymous]
        [HttpGet("most-viewed")]
        public async Task<ActionResult<ApiResponse<List<ArticleQueryModel>>>> MostViewedArticles(int take = 10)
        {
            var articles = await _articleQuery.GetMostViewedArticles(take);

            if (articles == null || !articles.Any())
                return NotFound(ApiResponse<List<ArticleQueryModel>>
                    .Fail(message: "هیچ مقاله‌ای یافت نشد."));

            return Ok(ApiResponse<List<ArticleQueryModel>>.Ok(data: articles));
        }
    }
}