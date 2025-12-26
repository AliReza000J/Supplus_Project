using CommentManagement.Application.Contracts.Comment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServiceHost.Areas.Admin.Pages.Comments
{
    public class DetailsModel : PageModel
    {
        private readonly ICommentApplication _commentApplication;

        public DetailsModel(ICommentApplication commentApplication)
        {
            _commentApplication = commentApplication;
        }

        public CommentViewModel Comment;

        public async Task<IActionResult> OnGet(long id)
        {
            Comment = await _commentApplication.GetDetailsAsync(id);
            if (Comment == null)
                return RedirectToPage();


            return Page();
        }

        // Reply (admin)
        public async Task<IActionResult> OnPostReply(CommentViewModel comment, bool reset)
        {
            if (comment == null) return RedirectToPage();

            var result = await _commentApplication.Reply(comment.Id, comment.Reply ?? string.Empty);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return result.IsSucceeded ? RedirectToPage("./Index") : RedirectToPage();
        }
    }
}
