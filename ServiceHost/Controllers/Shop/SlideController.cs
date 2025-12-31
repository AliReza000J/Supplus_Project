using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Slide;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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

        /// <summary>
        /// Retrieves all active slides for the shop homepage.
        /// </summary>
        /// <remarks>
        /// This endpoint returns all enabled slides that should be displayed
        /// in the shop UI (e.g., homepage carousel or promotional banners).
        /// If no slides are available, the API responds with 404.
        /// </remarks>
        /// <response code="200">Slides retrieved successfully.</response>
        /// <response code="404">No slides were found.</response>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SlideQueryModel>>>> GetSlides(CancellationToken cancellation)
        {
            // Retrieve the slide list from the query service
            var slides = await _slideQuery.GetSlides(cancellation);

            // Return a 404 response if no slides exist
            if (slides == null || !slides.Any())
                return NotFound(ApiResponse<List<SlideQueryModel>>
                    .Fail(message: "هیچ اسلایدی برای نمایش یافت نشد."));

            // Return the slide list wrapped in a standard API response object
            return Ok(ApiResponse<List<SlideQueryModel>>.Ok(data: slides));
        }
    }
}