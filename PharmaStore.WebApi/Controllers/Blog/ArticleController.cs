using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Article;
using _01_PharmaPlusQuery.Contracts.ArticleCategory;
using BlogManagement.Application.Contracts.Article;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaShared.Web.Filters;
using System.Security.Claims;

namespace PharmaStore.WebApi.Controllers.Blog
{

    /// <summary>
    /// Controller for managing website articles.
    /// </summary>
    /// <remarks>
    /// Contains endpoints for retrieving article details, latest arrivals, searching, top-rated, and most-viewed articles.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly ILogger<ArticleController> _logger;
        private readonly IArticleQuery _articleQuery;
        private readonly IArticleCategoryQuery _articleCategoryQuery;
        private readonly IArticleApplication _articleApplication;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleController"/> class.
        /// </summary>
        public ArticleController(IArticleQuery articleQuery, IArticleCategoryQuery articleCategoryQuery, ILogger<ArticleController> logger, IArticleApplication articleApplication)
        {
            _articleQuery = articleQuery;
            _articleCategoryQuery = articleCategoryQuery;
            _logger = logger;
            _articleApplication = articleApplication;
        }

        /// <summary>
        /// Retrieves the details of an article based on its slug and category slug.
        /// </summary>
        /// <remarks>
        /// Returns the complete details of the article. It also implements a unique view counting mechanism using cookies.
        /// If the user is authenticated, their user ID is recorded alongside the view count.
        /// </remarks>
        /// <param name="categorySlug">The slug of the article's category.</param>
        /// <param name="articleSlug">The slug of the article.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>Comprehensive article information including text, image, author details, etc.</returns>
        /// <response code="200">Operation successful. Article details returned.</response>
        /// <response code="400">Bad request. Invalid article slug provided.</response>
        /// <response code="404">Not found. The article or category was not found, or they do not match.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [AllowAnonymous]
        [HttpGet("{categorySlug}/{articleSlug}")]
        [ProducesResponseType(typeof(ApiResponse<ArticleQueryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ArticleQueryModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ArticleQueryModel>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ArticleQueryModel>>> GetArticleDetails(string articleSlug, string categorySlug, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(articleSlug))
                return BadRequest(ApiResponse<ArticleQueryModel>.Fail(message: "مسیر مقاله معتبر نیست."));

            var article = await _articleQuery.GetArticleDetails(articleSlug, cancellation);
            if (article == null)
                return NotFound(ApiResponse<ArticleQueryModel>.Fail(message: "مقاله یافت نشد."));

            var category = await _articleCategoryQuery.GetArticleCategory(categorySlug, cancellation);
            if (category == null || article.CategorySlug != category.Slug)
                return NotFound(ApiResponse<ArticleQueryModel>.Fail(message: "مقاله با مسیر گروه مقاله موردنظر مطابقت ندارد."));

            var articleId = await _articleQuery.GetArticleIdBySlug(articleSlug, cancellation);
            if (articleId <= 0)
                return NotFound(ApiResponse<ArticleQueryModel>.Fail(message: "شناسه مقاله معتبر نیست."));

            string cookieName = $"av_{articleId}";

            long userId = 0;
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(idClaim, out var parsedId))
                userId = parsedId;

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "ANON";

            if (!Request.Cookies.ContainsKey(cookieName))
            {
                var now = DateTime.UtcNow;

                var op = await _articleApplication.IncrementArticleViewCountAsync(articleId, userId, cancellation);
                if (op.IsSucceeded)
                {
                    Response.Cookies.Append(
                    cookieName,
                    $"2ck-{username}_{now}",
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
        /// Retrieves a list of the latest published articles.
        /// </summary>
        /// <remarks>
        /// Returns a list of recently registered and published articles (typically used for the homepage).
        /// </remarks>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>A list of the latest articles.</returns>
        /// <response code="200">Operation successful. List of articles returned.</response>
        /// <response code="404">Not found. No articles are currently registered in the system.</response>
        [AllowAnonymous]
        [HttpGet("latest-arrivals")]
        [ProducesResponseType(typeof(ApiResponse<List<ArticleQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ArticleQueryModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ArticleQueryModel>>>> GetLatestArrivals(CancellationToken cancellation)
        {
            var articles = await _articleQuery.GetLatestArticles(cancellation);

            if (articles == null || !articles.Any())
                return NotFound(ApiResponse<List<ArticleQueryModel>>.Fail(message: "هیچ مقاله‌ای یافت نشد."));

            return Ok(ApiResponse<List<ArticleQueryModel>>.Ok(data: articles));
        }

        /// <summary>
        /// Searches for articles based on a keyword.
        /// </summary>
        /// <remarks>
        /// Provides search functionality across article titles, texts, and keywords with pagination support.
        /// This endpoint has a rate limit of 30 requests per 60 seconds.
        /// </remarks>
        /// <param name="value">The search query keyword.</param>
        /// <param name="page">The current page number (default: 1).</param>
        /// <param name="pageSize">The number of articles per page (default: 10).</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>A paginated result of the found articles.</returns>
        /// <response code="200">Operation successful. Search results returned (even if empty).</response>
        /// <response code="400">Bad request. The search query is missing or empty.</response>
        [AllowAnonymous]
        [HttpGet("search")]
        [RateLimit(Limit = 30, Seconds = 60)]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ArticleQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ArticleQueryModel>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PagedResult<ArticleQueryModel>>>> Search(
            string value, int page = 1, int pageSize = 10, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(value))
                return BadRequest(ApiResponse<PagedResult<ArticleQueryModel>>
                    .Fail("عبارت جستجو نمی‌تواند خالی باشد."));

            var result = await _articleQuery.Search(value, page, pageSize, cancellation);

            return Ok(ApiResponse<PagedResult<ArticleQueryModel>>.Ok(
                result,
                result.Items.Any()
                    ? "جستجو با موفقیت انجام شد."
                    : $"هیچ مقاله‌ای برای عبارت «{value}» یافت نشد."
            ));
        }

        /// <summary>
        /// Retrieves a list of top-rated articles.
        /// </summary>
        /// <remarks>
        /// Returns a list of articles that have received the highest ratings from users.
        /// </remarks>
        /// <param name="take">The number of articles to retrieve (default: 10).</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>A list of top-rated articles.</returns>
        /// <response code="200">Operation successful. Top-rated articles returned.</response>
        /// <response code="404">Not found. No articles found.</response>
        [AllowAnonymous]
        [HttpGet("top-rated")]
        [ProducesResponseType(typeof(ApiResponse<List<ArticleQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ArticleQueryModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ArticleQueryModel>>>> TopRatedArticles(int take = 10, CancellationToken cancellation = default)
        {
            var articles = await _articleQuery.GetTopRatedArticles(take, cancellation);

            if (articles == null || !articles.Any())
                return NotFound(ApiResponse<List<ArticleQueryModel>>
                    .Fail(message: "هیچ مقاله‌ای یافت نشد."));

            return Ok(ApiResponse<List<ArticleQueryModel>>.Ok(data: articles));
        }

        /// <summary>
        /// Retrieves a list of the most viewed articles.
        /// </summary>
        /// <remarks>
        /// Returns a list of articles that have the highest view count.
        /// </remarks>
        /// <param name="take">The number of articles to retrieve (default: 10).</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>A list of the most viewed articles.</returns>
        /// <response code="200">Operation successful. Most viewed articles returned.</response>
        /// <response code="404">Not found. No articles found.</response>
        [AllowAnonymous]
        [HttpGet("most-viewed")]
        [ProducesResponseType(typeof(ApiResponse<List<ArticleQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ArticleQueryModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ArticleQueryModel>>>> MostViewedArticles(int take = 10, CancellationToken cancellation = default)
        {
            var articles = await _articleQuery.GetMostViewedArticles(take, cancellation);

            if (articles == null || !articles.Any())
                return NotFound(ApiResponse<List<ArticleQueryModel>>
                    .Fail(message: "هیچ مقاله‌ای یافت نشد."));

            return Ok(ApiResponse<List<ArticleQueryModel>>.Ok(data: articles));
        }
    }

}