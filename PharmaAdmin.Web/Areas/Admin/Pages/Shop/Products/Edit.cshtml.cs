using _0_Framework.Application;
using _0_Framework.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopManagement.Application.Contracts.Product;
using ShopManagement.Application.Contracts.ProductCategory;
using ShopManagement.Configuration.Permissions;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Shop.Products
{
    public class EditModel : PageModel
    {
        public EditProduct? Command;
        public SelectList ProductCategories;

        private readonly IProductApplication _productApplication;
        private readonly IProductCategoryApplication _productCategoryApplication;
        private readonly ICacheService _cacheService;

        public EditModel(IProductApplication productApplication, IProductCategoryApplication productCategoryApplication, ICacheService cacheService)
        {
            _productApplication = productApplication;
            _productCategoryApplication = productCategoryApplication;
            _cacheService = cacheService;
        }

        [NeedsPermission(ShopPermissions.EditProduct)]
        public async Task OnGet(long id, CancellationToken cancellation)
        {
            Command = await _productApplication.GetDetails(id, cancellation);
            Command.Categories = await _productCategoryApplication.GetLeafCategories(cancellation);
            ProductCategories = new SelectList(await _productCategoryApplication.GetLeafCategories(cancellation), "Id", "Name");
        }

        [NeedsPermission(ShopPermissions.EditProduct)]
        public async Task<IActionResult> OnPost(EditProduct command, CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return new JsonResult(new { IsSucceeded = false, Message = "ورودی نامعتبر است." });

            var result = await _productApplication.Edit(command, cancellation);
            _cacheService.Remove($"product:base:{command.Slug}");
            if (result.IsSucceeded)
            {
                TempData["Toast.Success"] = "true";
                TempData["Toast.Message"] = result.Message;
                return RedirectToPage("./Index");
            }

            TempData["Toast.Success"] = "false";
            TempData["Toast.Message"] = result.Message;
            return Page();
        }
    }
}
