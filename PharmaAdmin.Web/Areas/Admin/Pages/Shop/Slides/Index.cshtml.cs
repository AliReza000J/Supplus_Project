using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopManagement.Application.Contracts.Slide;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Shop.Slides
{
    public class IndexModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        public List<SlideViewModel> Slides;

        private readonly ISlideApplication _slideApplication;

        public IndexModel(ISlideApplication slideApplication)
        {
            _slideApplication = slideApplication;
        }

        public async Task OnGet(CancellationToken cancellation)
        {
            Slides = await _slideApplication.GetList(cancellation);
        }

        public IActionResult OnGetCreate()
        {
            var command = new CreateSlide();
            return Partial("./Create", command);
        }

        public async Task<JsonResult> OnPostCreate(CreateSlide command, CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return new JsonResult(new { IsSucceeded = false, Message = "ورودی نامعتبر است." });

            var result = await _slideApplication.Create(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetEdit(long id, CancellationToken cancellation)
        {
            var slide = await _slideApplication.GetDetails(id, cancellation);
            return Partial("Edit", slide);
        }

        public async Task<JsonResult> OnPostEdit(EditSlide command, CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return new JsonResult(new { IsSucceeded = false, Message = "ورودی نامعتبر است." });

            var result = await _slideApplication.Edit(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetRemove(long id, CancellationToken cancellation)
        {
            var result = await _slideApplication.Remove(id, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetRestore(long id, CancellationToken cancellation)
        {
            var result = await _slideApplication.Restore(id, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index");
        }
    }
}
