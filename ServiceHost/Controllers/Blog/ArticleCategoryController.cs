using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.ArticleCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServiceHost.Controllers.Blog
{
    /// <summary>
    /// Manages article categories.
    /// </summary>
    /// <remarks>
    /// This controller provides access to article category information,
    /// including listing all categories and retrieving individual categories by slug.
    /// </remarks>
    [Route("blog/[controller]")]
    [ApiController]
    public class ArticleCategoryController : ControllerBase
    {
        private readonly IArticleCategoryQuery _articleCategoryQuery;
        private readonly ILogger<ArticleCategoryController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleCategoryController"/> class.
        /// </summary>
        /// <param name="logger">Logger instance used for logging operations.</param>
        /// <param name="articleCategoryQuery">Service used to query article category data.</param>
        public ArticleCategoryController(
            ILogger<ArticleCategoryController> logger,
            IArticleCategoryQuery articleCategoryQuery)
        {
            _logger = logger;
            _articleCategoryQuery = articleCategoryQuery;
        }

        /// <summary>
        /// Retrieves all article categories.
        /// </summary>
        /// <remarks>
        /// Returns a list of all available article categories.
        /// If no categories exist, a 404 response is returned.
        /// </remarks>
        /// <returns>
        /// A list of categories wrapped in <see cref="ApiResponse{T}"/>.
        /// </returns>
        /// <response code="200">Categories retrieved successfully.</response>
        /// <response code="404">No categories were found.</response>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ArticleCategoryQueryModel>>>> GetAllCategories()
        {
            var result = await _articleCategoryQuery.GetArticleCategories();

            if (result == null || !result.Any())
                return NotFound(ApiResponse<List<ArticleCategoryQueryModel>>
                    .Fail(message: "هیچ دسته‌بندی برای مقالات یافت نشد."));

            return Ok(ApiResponse<List<ArticleCategoryQueryModel>>.Ok(result));
        }

        /// <summary>
        /// Retrieves a specific article category by slug.
        /// </summary>
        /// <param name="slug">The slug of the category.</param>
        /// <remarks>
        /// Returns detailed information about a specific category using its slug.
        /// If the slug is invalid or the category does not exist, an appropriate error is returned.
        /// </remarks>
        /// <returns>
        /// An <see cref="ArticleCategoryQueryModel"/> wrapped in <see cref="ApiResponse{T}"/>.
        /// </returns>
        /// <response code="200">Category data retrieved successfully.</response>
        /// <response code="400">The provided slug is invalid.</response>
        /// <response code="404">No category exists with the specified slug.</response>
        [AllowAnonymous]
        [HttpGet("{slug}")]
        public async Task<ActionResult<ApiResponse<ArticleCategoryQueryModel>>> GetCategoryBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest(ApiResponse<ArticleCategoryQueryModel>
                    .Fail("مسیر نامعتبر."));

            var result = await _articleCategoryQuery.GetArticleCategory(slug);

            if (result == null)
                return NotFound(ApiResponse<ArticleCategoryQueryModel>
                    .Fail($"دسته‌بندی «{slug}» یافت نشد."));

            return Ok(ApiResponse<ArticleCategoryQueryModel>.Ok(result));
        }
    }
}