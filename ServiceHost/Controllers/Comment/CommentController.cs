using _0_Framework.Application;
using CommentManagement.Application.Contracts.Comment;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ServiceHost.Controllers
{
    /// <summary>
    /// Provides authenticated operations for managing user comments,
    /// including creation, editing, deletion, and reactions (like/dislike).
    /// </summary>
    /// <remarks>
    /// All operations require JWT authentication.
    /// Each action automatically reads the user ID from the JWT token.
    /// </remarks>
    [Route("[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ILogger<CommentController> _logger;
        private readonly ICommentApplication _commentApplication;

        /// <summary>
        /// Initializes the <see cref="CommentController"/>.
        /// </summary>
        /// <param name="commentApplication">Application service for comment operations.</param>
        /// <param name="logger">Logger for diagnostics and error tracking.</param>
        public CommentController(ICommentApplication commentApplication, ILogger<CommentController> logger)
        {
            _commentApplication = commentApplication;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new comment by the authenticated user.
        /// </summary>
        /// <param name="command">The comment creation DTO.</param>
        /// <remarks>
        /// The user ID and username are extracted from the JWT token.
        /// </remarks>
        /// <returns>An API response indicating success or failure.</returns>
        /// <response code="200">Comment added successfully.</response>
        /// <response code="400">Invalid or missing comment data.</response>
        /// <response code="401">User is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<object>>> Add([FromBody] AddComment command)
        {
            if (command == null)
                return BadRequest(ApiResponse<object>.Fail(message: "اطلاعات کامنت ارسال نشده است."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var usernameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var op = await _commentApplication.AddAsync(command, userIdClaim, usernameClaim);
            var result = new ApiResponse<object>
            {
                Success = op.IsSucceeded,
                Message = op.Message
            };
            if (op.IsSucceeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Edits a comment owned by the authenticated user.
        /// </summary>
        /// <param name="command">The comment edit DTO.</param>
        /// <returns>An API response describing the result.</returns>
        /// <remarks>
        /// Only the owner of a comment is allowed to edit it.
        /// </remarks>
        /// <response code="200">Comment updated successfully.</response>
        /// <response code="400">Invalid request body.</response>
        /// <response code="401">User is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit")]
        public async Task<ActionResult<ApiResponse<object>>> Edit([FromBody] EditComment command)
        {
            if (command == null)
                return BadRequest(ApiResponse<object>.Fail(message: "داده‌های ارسالی معتبر نیستند."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var op = await _commentApplication.EditAsync(command, userIdClaim);
            var result = new ApiResponse<object>
            {
                Success = op.IsSucceeded,
                Message = op.Message
            };
            if (op.IsSucceeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Deletes a comment owned by the authenticated user.
        /// </summary>
        /// <param name="commentId">The ID of the comment to delete.</param>
        /// <returns>An API response describing the result.</returns>
        /// <response code="200">Comment deleted successfully.</response>
        /// <response code="400">Invalid comment ID.</response>
        /// <response code="401">User is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete/{commentId:long}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(long commentId)
        {
            if (commentId <= 0)
                return BadRequest(ApiResponse<object>.Fail(message: "شناسه کامنت نامعتبر است."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var op = await _commentApplication.DeleteAsync(commentId, userIdClaim);
            var result = new ApiResponse<object>
            {
                Success = op.IsSucceeded,
                Message = op.Message
            };
            if (op.IsSucceeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Adds a "like" to a comment by the authenticated user.
        /// </summary>
        /// <param name="commentId">The target comment ID.</param>
        /// <returns>An API response describing the result.</returns>
        /// <response code="200">Like added successfully.</response>
        /// <response code="400">Invalid comment ID.</response>
        /// <response code="401">User is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("like/{commentId:long}")]
        public async Task<ActionResult<ApiResponse<object>>> Like(long commentId)
        {
            if (commentId <= 0)
                return BadRequest(ApiResponse<object>.Fail(message: "شناسه کامنت نامعتبر است."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var op = await _commentApplication.LikeAsync(commentId, userIdClaim);
            var result = new ApiResponse<object>
            {
                Success = op.IsSucceeded,
                Message = op.Message
            };

            if (!op.IsSucceeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Adds a "dislike" to a comment by the authenticated user.
        /// </summary>
        /// <param name="commentId">The target comment ID.</param>
        /// <returns>An API response describing the result.</returns>
        /// <response code="200">Dislike added successfully.</response>
        /// <response code="400">Invalid comment ID.</response>
        /// <response code="401">User is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("dislike/{commentId:long}")]
        public async Task<ActionResult<ApiResponse<object>>> Dislike(long commentId)
        {
            if (commentId <= 0)
                return BadRequest(ApiResponse<object>.Fail(message: "شناسه کامنت نامعتبر است."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var op = await _commentApplication.DislikeAsync(commentId, userIdClaim);
            var result = new ApiResponse<object>
            {
                Success = op.IsSucceeded,
                Message = op.Message
            };

            if (!op.IsSucceeded)
                return BadRequest(result);

            return Ok(result);
        }
    }
}