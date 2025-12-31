using _0_Framework.Application;
using _0_Framework.Infrastructure;
using InventoryManagement.Infrastructure.Configuration.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using ShopManagement.Application.Contracts.Product;
using ShopManagement.Application.Contracts.ProductCategory;
using ShopManagement.Configuration.Permissions;

namespace ServiceHost.Areas.Admin.Pages.Shop.Products
{
    public class EditModel : PageModel
    {
        public EditProduct Command;
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
        public async Task OnGet(long id)
        {
            Command = await _productApplication.GetDetails(id);
            Command.Categories = await _productCategoryApplication.GetLeafCategories();
            ProductCategories = new SelectList(await _productCategoryApplication.GetLeafCategories(), "Id", "Name");
        }

        [NeedsPermission(ShopPermissions.EditProduct)]
        public async Task<IActionResult> OnPost(EditProduct command)
        {
            if (!ModelState.IsValid)
                return new JsonResult(new { IsSucceeded = false, Message = "ورودی نامعتبر است." });

            var result = await _productApplication.Edit(command);
            _cacheService.Remove($"product:base:{command.Slug}");
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index", result);
        }
    }
}
