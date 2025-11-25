using CommentManagement.Application.Contracts.Comment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServiceHost.Areas.Admin.Pages.Comments
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

        public async Task OnGet()
        {
            if (SearchModel.ShowPendingOnly)
            {
                Comments = await _commentApplication.SearchAsync(new CommentSearchModel
                {
                    ShowPendingOnly = true
                });
            }
            else
            {
                Comments = await _commentApplication.SearchAsync(SearchModel);
            }

        }

        public async Task<IActionResult> OnGetCancel(long id)
        {
            var result = await _commentApplication.Cancel(id);

            if (result.IsSucceeded)
                return RedirectToPage();

            Message = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetConfirm(long id, bool reset)
        {
            var result = await _commentApplication.Confirm(id);

            if (result.IsSucceeded)
                return RedirectToPage();

            Message = result.Message;
            return RedirectToPage();
        }
    }
}