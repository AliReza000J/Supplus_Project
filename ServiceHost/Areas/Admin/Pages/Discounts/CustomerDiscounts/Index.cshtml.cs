using DiscountManagement.Application.Contracts.CustomerDiscount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopManagement.Application.Contracts.Product;

namespace ServiceHost.Areas.Administration.Pages.Discounts.CustomerDiscounts
{
    //[Authorize(Roles = Roles.Administator)]
    public class IndexModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        public CustomerDiscountSearchModel SearchModel;
        public List<CustomerDiscountViewModel> CustomerDiscounts;
        public SelectList Products;

        private readonly IProductApplication _productApplication;
        private readonly ICustomerDiscountApplication _customerDiscountApplication;

        public IndexModel(IProductApplication ProductApplication, ICustomerDiscountApplication customerDiscountApplication)
        {
            _productApplication = ProductApplication;
            _customerDiscountApplication = customerDiscountApplication;
        }

        public async Task OnGet(CustomerDiscountSearchModel searchModel, CancellationToken cancellation)
        {
            Products = new SelectList(await _productApplication.GetProducts(cancellation), "Id", "Name");
            CustomerDiscounts = await _customerDiscountApplication.Search(searchModel, cancellation);
        }

        public async Task<IActionResult> OnGetCreate(CancellationToken cancellation)
        {
            var command = new DefineCustomerDiscount
            {
                Products = await _productApplication.GetProducts(cancellation)
            };
            return Partial("./Create", command);
        }

        public async Task<JsonResult> OnPostCreate(DefineCustomerDiscount command, CancellationToken cancellation)
        {
            var result = await _customerDiscountApplication.Define(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetEdit(long id, CancellationToken cancellation)
        {
            var customerDiscount = await _customerDiscountApplication.GetDetails(id, cancellation);
            customerDiscount.Products = await _productApplication.GetProducts(cancellation);
            return Partial("Edit", customerDiscount);
        }

        public async Task<JsonResult> OnPostEdit(EditCustoemrDiscount command, CancellationToken cancellation)
        {
            var result = await _customerDiscountApplication.Edit(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }
    }
}
