using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Slide;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PharmaStore.WebApi.Controllers.Shop
{
    /// <summary>
    /// Endpoints for retrieving slides.
    /// Typically used by the public storefront to display image carousels or banners on the homepage.
    /// </summary>
    [Route("api/[controller]")]
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

        /// <summary>
        /// Retrieves a list of active slides to be displayed.
        /// </summary>
        /// <remarks>
        /// This endpoint is publicly accessible and fetches the currently active slides configured in the system.
        /// </remarks>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>A list of slide details.</returns>
        /// <response code="200">Returns the list of slides successfully.</response>
        /// <response code="404">If no active slides are found in the system.</response>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<SlideQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<SlideQueryModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<SlideQueryModel>>>> GetSlides(CancellationToken cancellation)
        {
            var slides = await _slideQuery.GetSlides(cancellation);

            if (slides == null || !slides.Any())
                return NotFound(ApiResponse<List<SlideQueryModel>>
                    .Fail(message: "هیچ اسلایدی برای نمایش یافت نشد."));

            return Ok(ApiResponse<List<SlideQueryModel>>.Ok(data: slides));
        }
    }
}