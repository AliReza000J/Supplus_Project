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

        public IActionResult OnGet(long id)
        {
            Comment = _commentApplication.GetDetails(id);
            if (Comment == null)
                return RedirectToPage("./Index"); // or NotFound


            return Page();
        }

        //// Confirm
        //public IActionResult OnPostConfirm(long id)
        //{
        //    var result = _commentApplication.Confirm(id);
        //    if (result.IsSucceeded)
        //        return RedirectToPage();

        //    TempData["Error"] = result.Message;
        //    return RedirectToPage();
        //}

        //// Cancel / Reject
        //public IActionResult OnPostCancel(long id)
        //{
        //    var result = _commentApplication.Cancel(id);
        //    if (result.IsSucceeded)
        //        return RedirectToPage();

        //    TempData["Error"] = result.Message;
        //    return RedirectToPage();
        //}

        // Reply (admin)
        public IActionResult OnPostReply(CommentViewModel comment)
        {
            if (comment == null) return RedirectToPage();

            var result = _commentApplication.Reply(comment.Id, comment.Reply ?? string.Empty);
            TempData["Message"] = result.Message;
            return RedirectToPage();
        }
    }
}
