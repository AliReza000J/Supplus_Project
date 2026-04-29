using DiscountManagement.Application.Contracts.Discount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Discounts
{
    public class IndexModel : PageModel
    {
        [TempData] public string Message { get; set; }
        public DiscountSearchModel SearchModel;
        public List<DiscountViewModel> Discounts;

        public SelectList Types;
        public SelectList ApplyTypes;

        private readonly IDiscountApplication _discountApplication;

        public IndexModel(IDiscountApplication discountApplication)
        {
            _discountApplication = discountApplication;
        }

        public async Task OnGet(DiscountSearchModel searchModel, CancellationToken cancellation)
        {
            Types = new SelectList(new List<SelectListItem>
            {
                new() { Value = "1", Text = "خودکار" },
                new() { Value = "2", Text = "کد تخفیف" }
            }, "Value", "Text");

            ApplyTypes = new SelectList(new List<SelectListItem>
            {
                new() { Value = "1", Text = "روی محصول" },
                new() { Value = "2", Text = "کل سفارش" }
            }, "Value", "Text");

            Discounts = await _discountApplication.Search(searchModel, cancellation);
        }

        public async Task<IActionResult> OnGetDeactivate(long id, CancellationToken cancellation)
        {
            var result = await _discountApplication.ToggleActiveStatus(id, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetActivate(long id, CancellationToken cancellation)
        {
            var result = await _discountApplication.ToggleActiveStatus(id, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index");
        }
    }
}
