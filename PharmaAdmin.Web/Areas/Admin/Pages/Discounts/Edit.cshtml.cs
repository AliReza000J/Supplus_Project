using DiscountManagement.Application.Contracts.Discount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopManagement.Application.Contracts.Product;
using ShopManagement.Application.Contracts.ProductCategory;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Discounts
{
    public class EditModel : PageModel
    {
        public EditDiscount Command;
        public SelectList Products;
        public SelectList Categories;
        public List<string> Brands;

        private readonly IProductApplication _productApplication;
        private readonly IProductCategoryApplication _categoryApplication;
        private readonly IDiscountApplication _discountApplication;

        public EditModel(IProductApplication productApplication,
            IProductCategoryApplication categoryApplication, IDiscountApplication discountApplication)
        {
            _productApplication = productApplication;
            _categoryApplication = categoryApplication;
            _discountApplication = discountApplication;
        }

        public async Task OnGet(long id, CancellationToken cancellation)
        {
            Command = await _discountApplication.GetDetails(id, cancellation) ?? new();

            Products = new SelectList(await _productApplication.GetProducts(cancellation), "Id", "Name");
            Categories = new SelectList(await _categoryApplication.GetProductCategories(cancellation), "Id", "Name");
            Brands = await _productApplication.GetDistinctBrands(cancellation) ?? new();
        }

        public async Task<IActionResult> OnPost(EditDiscount command, CancellationToken cancellation)
        {
            var result = await _discountApplication.Edit(command, cancellation);
            if (result.IsSucceeded)
            {
                TempData["Toast.Success"] = "true";
                TempData["Toast.Message"] = result.Message;
                return RedirectToPage("./Index");
            }

            TempData["Toast.Success"] = "false";
            TempData["Toast.Message"] = result.Message;

            Products = new SelectList(await _productApplication.GetProducts(cancellation), "Id", "Name");
            return Page();
        }
    }
}
