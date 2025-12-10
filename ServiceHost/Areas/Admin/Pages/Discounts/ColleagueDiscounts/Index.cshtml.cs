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

        public async Task OnGet(ColleagueDiscountSearchModel searchModel)
        {
            Products = new SelectList(await _productApplication.GetProducts(), "Id", "Name");
            ColleagueDiscounts = await _colleagueDiscountApplication.Search(searchModel);
        }

        public async Task<IActionResult> OnGetCreate()
        {
            var command = new DefineColleagueDiscount
            {
                Products = await _productApplication.GetProducts()
            };
            return Partial("./Create", command);
        }

        public async Task<JsonResult> OnPostCreate(DefineColleagueDiscount command)
        {
            var result = await _colleagueDiscountApplication.Define(command);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetEdit(long id)
        {
            var colleagueDiscount = await _colleagueDiscountApplication.GetDetails(id);
            colleagueDiscount.Products = await _productApplication.GetProducts();
            return Partial("Edit", colleagueDiscount);
        }

        public async Task<JsonResult> OnPostEdit(EditColleagueDiscount command)
        {
            var result = await _colleagueDiscountApplication.Edit(command);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetRemove(long id)
        {
            await _colleagueDiscountApplication.Remove(id);
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetRestore(long id)
        {
            await _colleagueDiscountApplication.Restore(id);
            return RedirectToPage("./Index");
        }
    }
}
