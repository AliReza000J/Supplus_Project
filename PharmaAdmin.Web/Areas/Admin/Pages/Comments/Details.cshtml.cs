using CommentManagement.Application.Contracts.Comment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Comments
{
    public class DetailsModel : PageModel
    {
        private readonly ICommentApplication _commentApplication;

        public DetailsModel(ICommentApplication commentApplication)
        {
            _commentApplication = commentApplication;
        }

        public CommentViewModel Comment;

        public async Task<IActionResult> OnGet(long id, CancellationToken cancellation)
        {
            Comment = await _commentApplication.GetDetailsAsync(id, cancellation);
            if (Comment == null)
                return RedirectToPage();


            return Page();
        }

        // Reply (admin)
        public async Task<IActionResult> OnPostReply(CommentViewModel comment, bool reset, CancellationToken cancellation)
        {
            if (comment == null) return RedirectToPage();

            var result = await _commentApplication.Reply(comment.Id, comment.Reply ?? string.Empty, cancellation);
            if (result.IsSucceeded)
            {
                TempData["Toast.Success"] = "true";
                TempData["Toast.Message"] = result.Message;
                return RedirectToPage("./Index");
            }

            TempData["Toast.Success"] = "false";
            TempData["Toast.Message"] = result.Message;
            return Page();
        }
    }
}
