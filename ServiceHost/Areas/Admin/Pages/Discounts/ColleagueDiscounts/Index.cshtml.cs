using DiscountManagement.Application.Contracts.ColleagueDiscount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopManagement.Application.Contracts.Product;

namespace ServiceHost.Areas.Admin.Pages.Discounts.ColleagueDiscounts
{
    //[Authorize(Roles = Roles.Administator)]
    public class IndexModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        public ColleagueDiscountSearchModel SearchModel;
        public List<ColleagueDiscountViewModel> ColleagueDiscounts;
        public SelectList Products;

        private readonly IProductApplication _productApplication;
        private readonly IColleagueDiscountApplication _colleagueDiscountApplication;

        public IndexModel(IProductApplication ProductApplication, IColleagueDiscountApplication colleagueDiscountApplication)
        {
            _productApplication = ProductApplication;
            _colleagueDiscountApplication = colleagueDiscountApplication;
        }

        public async Task OnGet(ColleagueDiscountSearchModel searchModel, CancellationToken cancellation)
        {
            Products = new SelectList(await _productApplication.GetProducts(cancellation), "Id", "Name");
            ColleagueDiscounts = await _colleagueDiscountApplication.Search(searchModel, cancellation);
        }

        public async Task<IActionResult> OnGetCreate(CancellationToken cancellation)
        {
            var command = new DefineColleagueDiscount
            {
                Products = await _productApplication.GetProducts(cancellation)
            };
            return Partial("./Create", command);
        }

        public async Task<JsonResult> OnPostCreate(DefineColleagueDiscount command, CancellationToken cancellation)
        {
            var result = await _colleagueDiscountApplication.Define(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetEdit(long id, CancellationToken cancellation)
        {
            var colleagueDiscount = await _colleagueDiscountApplication.GetDetails(id, cancellation);
            colleagueDiscount.Products = await _productApplication.GetProducts(cancellation);
            return Partial("Edit", colleagueDiscount);
        }

        public async Task<JsonResult> OnPostEdit(EditColleagueDiscount command, CancellationToken cancellation)
        {
            var result = await _colleagueDiscountApplication.Edit(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetRemove(long id, CancellationToken cancellation)
        {
            var result = await _colleagueDiscountApplication.Remove(id, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetRestore(long id, CancellationToken cancellation)
        {
            var result = await _colleagueDiscountApplication.Restore(id, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index");
        }
    }
}
