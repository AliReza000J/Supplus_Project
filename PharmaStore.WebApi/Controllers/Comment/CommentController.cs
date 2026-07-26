using _0_Framework.Application;
using CommentManagement.Application.Contracts.Comment;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PharmaStore.WebApi.Controllers
{

    /// <summary>
    /// Manages user comments for various entities like products or articles.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentApplication _commentApplication;

        /// <summary>
        /// Initializes a new instance of the CommentController.
        /// </summary>
        public CommentController(ICommentApplication commentApplication)
        {
            _commentApplication = commentApplication;
        }

        /// <summary>
        /// Adds a new comment to an entity (e.g., a product or an article).
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to post a comment and a rating for a specific record.
        /// The user's ID and username are extracted from the authentication token.
        /// <br/>
        /// </remarks>
        /// <param name="command">The details of the comment to be added.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The comment was added successfully.</response>
        /// <response code="400">Bad Request. The input data is invalid. Check the validation messages in the response.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("add")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> Add([FromBody] AddComment command, CancellationToken cancellation)
        {
            if (command == null)
                return BadRequest(ApiResponse<object>.Fail(message: "اطلاعات کامنت ارسال نشده است."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var usernameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(usernameClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var userId = long.Parse(userIdClaim);
            var result = await _commentApplication.AddAsync(command, userId, usernameClaim, cancellation);
            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: result.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }



        /// <summary>
        /// Edits an existing comment.
        /// </summary>
        /// <remarks>
        /// Allows an authenticated user to edit their own comment. The user can update the message, the rating, or both.
        /// An attempt to edit a comment that does not belong to the user will result in a failed operation.
        /// <br/>
        /// </remarks>
        /// <param name="command">The details of the comment to be edited.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The comment was edited successfully.</response>
        /// <response code="400">Bad Request. The input data is invalid or the user does not have permission to edit this comment.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> Edit([FromBody] EditComment command, CancellationToken cancellation)
        {
            if (command == null)
                return BadRequest(ApiResponse<object>.Fail(message: "داده‌های ارسالی معتبر نیستند."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var userId = long.Parse(userIdClaim);
            var result = await _commentApplication.EditAsync(command, userId, cancellation);
            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: result.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

        /// <summary>
        /// Deletes a comment.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to delete their own comment.
        /// An attempt to delete a comment that does not belong to the user will result in a failed operation.
        /// </remarks>
        /// <param name="commentId">The ID of the comment to be deleted.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The comment was deleted successfully.</response>
        /// <response code="400">Bad Request. The comment ID is invalid or the user does not have permission to delete this comment.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete/{commentId:long}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> Delete(long commentId, CancellationToken cancellation)
        {
            if (commentId <= 0)
                return BadRequest(ApiResponse<object>.Fail(message: "شناسه کامنت نامعتبر است."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var userId = long.Parse(userIdClaim);
            var result = await _commentApplication.DeleteAsync(commentId, userId, cancellation);
            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: result.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

        /// <summary>
        /// Likes a comment.
        /// </summary>
        /// <remarks>
        /// Allows an authenticated user to add a 'like' to a specific comment.
        /// If the user has already liked the comment, the operation might fail or have no effect.
        /// <br/>
        /// </remarks>
        /// <param name="commentId">The ID of the comment to like.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The comment was liked successfully.</response>
        /// <response code="400">Bad Request. The comment ID is invalid or the operation failed (e.g., already liked).</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("like/{commentId:long}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> Like(long commentId, CancellationToken cancellation)
        {
            if (commentId <= 0)
                return BadRequest(ApiResponse<object>.Fail(message: "شناسه کامنت نامعتبر است."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var userId = long.Parse(userIdClaim);
            var result = await _commentApplication.LikeAsync(commentId, userId, cancellation);
            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: result.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

        /// <summary>
        /// Dislikes a comment.
        /// </summary>
        /// <remarks>
        /// Allows an authenticated user to add a 'dislike' to a specific comment.
        /// If the user has already disliked the comment, the operation might fail or have no effect.
        /// <br/>
        /// </remarks>
        /// <param name="commentId">The ID of the comment to dislike.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The comment was disliked successfully.</response>
        /// <response code="400">Bad Request. The comment ID is invalid or the operation failed (e.g., already disliked).</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("dislike/{commentId:long}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> Dislike(long commentId, CancellationToken cancellation)
        {
            if (commentId <= 0)
                return BadRequest(ApiResponse<object>.Fail(message: "شناسه کامنت نامعتبر است."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(ApiResponse<object>.Fail(message: "کاربر شناسایی نشد."));

            var userId = long.Parse(userIdClaim);
            var result = await _commentApplication.DislikeAsync(commentId, userId, cancellation);
            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: result.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }
    }
}