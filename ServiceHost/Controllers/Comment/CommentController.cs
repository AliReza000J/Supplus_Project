using CommentManagement.Application.Contracts.Comment;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ServiceHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ILogger<CommentController> _logger;
        private readonly ICommentApplication _commentApplication;

        public CommentController(ICommentApplication commentApplication, ILogger<CommentController> logger)
        {
            _commentApplication = commentApplication;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("add")]
        public IActionResult AddComment([FromBody] AddComment command)
        {
            if (command == null)
                return BadRequest("اطلاعات کامنت ارسال نشده است.");

            try
            {
                // userId JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("کاربر شناسایی نشد.");

                // username JWT
                var usernameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("کاربر شناسایی نشد.");

                var result = _commentApplication.Add(command, userIdClaim, usernameClaim);
                if (result.IsSucceeded)
                    return Ok(result);

                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا هنگام افزودن کامنت برای {OwnerRecordId}", command.OwnerRecordId);
                return StatusCode(500, "خطایی در ثبت کامنت رخ داد.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit")]
        public IActionResult Edit([FromBody] EditComment command)
        {
            if (command == null)
                return BadRequest("داده‌های ارسالی معتبر نیستند.");

            try
            {
                // userId JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("کاربر شناسایی نشد.");

                var result = _commentApplication.Edit(command, userIdClaim);
                if (result.IsSucceeded)
                    return Ok(result);

                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ویرایش کامنت {CommentId}", command.Id);
                return StatusCode(500, "خطای داخلی سرور هنگام ویرایش کامنت.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete/{id:long}")]
        public IActionResult Delete(long id)
        {
            if (id <= 0)
                return BadRequest("شناسه کامنت نامعتبر است.");

            try
            {
                // userId JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("کاربر شناسایی نشد.");

                var result = _commentApplication.Delete(id, userIdClaim);
                if (result.IsSucceeded)
                    return Ok(result);

                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف کامنت {CommentId}", id);
                return StatusCode(500, "خطای داخلی سرور هنگام حذف کامنت.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("like/{commentId:long}")]
        public IActionResult Like(long commentId)
        {
            if (commentId <= 0)
                return BadRequest("شناسه کامنت نامعتبر است.");

            try
            {
                // userId JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("کاربر شناسایی نشد.");

                //var userId = long.Parse(userIdClaim);

                var result = _commentApplication.Like(commentId, userIdClaim);

                if (!result.IsSucceeded)
                    return BadRequest(result.Message);

                return Ok(new { message = "لایک ثبت شد.", commentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در لایک کردن کامنت {CommentId}", commentId);
                return StatusCode(500, "خطایی در لایک کردن کامنت رخ داد.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("dislike/{commentId:long}")]
        public IActionResult Dislike(long commentId)
        {
            if (commentId <= 0)
                return BadRequest("شناسه کامنت نامعتبر است.");

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("کاربر شناسایی نشد.");

                //var userId = long.Parse(userIdClaim);

                var result = _commentApplication.Dislike(commentId, userIdClaim);

                if (!result.IsSucceeded)
                    return BadRequest(result.Message);

                return Ok(new { message = "دیسلایک ثبت شد.", commentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دیسلایک کردن کامنت {CommentId}", commentId);
                return StatusCode(500, "خطایی در دیسلایک کردن کامنت رخ داد.");
            }
        }

    }
}
