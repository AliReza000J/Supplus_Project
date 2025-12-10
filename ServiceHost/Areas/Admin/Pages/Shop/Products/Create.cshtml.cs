using _0_Framework.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopManagement.Application.Contracts.Product;
using ShopManagement.Application.Contracts.ProductCategory;
using ShopManagement.Configuration.Permissions;

namespace ServiceHost.Areas.Admin.Pages.Shop.Products
{
    public class CreateModel : PageModel
    {
        public CreateProduct Command;
        public SelectList ProductCategories;

        private readonly IProductApplication _productApplication;
        private readonly IProductCategoryApplication _productCategoryApplication;
        public CreateModel(IProductCategoryApplication productCategoryApplication, IProductApplication productApplication)
        {
            _productCategoryApplication = productCategoryApplication;
            _productApplication = productApplication;
        }

        [NeedsPermission(ShopPermissions.CreateProduct)]
        public async Task OnGet()
        {
            ProductCategories = new SelectList(await _productCategoryApplication.GetLeafCategories(), "Id", "Name");
        }

        [NeedsPermission(ShopPermissions.CreateProduct)]
        public async Task<IActionResult> OnPost(CreateProduct command)
        {
            command.Categories = await _productCategoryApplication.GetLeafCategories();
            if (!ModelState.IsValid)
                return new JsonResult(new { IsSucceeded = false, Message = "ورودی نامعتبر است." });

            var result = await _productApplication.Create(command);
            return RedirectToPage("./Index", result);
        }
    }
}
