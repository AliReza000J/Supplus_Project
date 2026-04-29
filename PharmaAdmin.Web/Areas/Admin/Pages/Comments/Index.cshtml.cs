using CommentManagement.Application.Contracts.Comment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Comments
{
    public class IndexModel : PageModel
    {
        [TempData] public string Message { get; set; }

        public List<CommentViewModel> Comments { get; set; }
        [BindProperty(SupportsGet = true)]
        public CommentSearchModel SearchModel { get; set; }

        private readonly ICommentApplication _commentApplication;

        public IndexModel(ICommentApplication commentApplication)
        {
            _commentApplication = commentApplication;
        }

        public async Task OnGet(CancellationToken cancellation)
        {
            if (SearchModel.ShowPendingOnly)
            {
                Comments = await _commentApplication.SearchAsync(new CommentSearchModel
                {
                    ShowPendingOnly = true
                }, cancellation);
            }
            else
            {
                Comments = await _commentApplication.SearchAsync(SearchModel, cancellation);
            }

        }

        public async Task<IActionResult> OnGetCancel(long id, CancellationToken cancellation)
        {
            var result = await _commentApplication.Cancel(id, cancellation);

            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetConfirm(long id, bool reset, CancellationToken cancellation)
        {
            var result = await _commentApplication.Confirm(id, cancellation);

            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage();
        }
    }
}