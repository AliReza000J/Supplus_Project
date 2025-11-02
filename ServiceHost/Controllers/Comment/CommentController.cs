using CommentManagement.Application.Contracts.Comment;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("add")]
        public IActionResult AddComment([FromBody] AddComment command)
        {
            if (command == null)
                return BadRequest("اطلاعات کامنت ارسال نشده است.");

            try
            {
                var result = _commentApplication.Add(command);
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

        [HttpPut("edit")]
        public IActionResult Edit([FromBody] EditComment command)
        {
            if (command == null)
                return BadRequest("داده‌های ارسالی معتبر نیستند.");

            try
            {
                var result = _commentApplication.Edit(command);
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

        [HttpDelete("delete/{id:long}")]
        public IActionResult Delete(long id)
        {
            try
            {
                var result = _commentApplication.Delete(id);
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

        [HttpPost("like/{commentId:long}")]
        public IActionResult Like(long commentId)
        {
            if (commentId <= 0)
                return BadRequest("شناسه کامنت نامعتبر است.");

            try
            {
                var result = _commentApplication.Like(commentId, User?.Identity?.Name ?? "anonymous");

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

        [HttpPost("dislike/{commentId:long}")]
        public IActionResult Dislike(long commentId)
        {
            if (commentId <= 0)
                return BadRequest("شناسه کامنت نامعتبر است.");

            try
            {
                var result = _commentApplication.Dislike(commentId, User?.Identity?.Name ?? "anonymous");

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
