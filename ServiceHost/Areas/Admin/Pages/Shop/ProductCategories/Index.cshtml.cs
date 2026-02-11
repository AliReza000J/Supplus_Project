using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopManagement.Application.Contracts.ProductCategory;
//using ShopManagement.Configuration.Permissions;

namespace ServiceHost.Areas.Admin.Pages.Shop.ProductCategories
{
    //[Authorize(Roles = "1, 3")]
    public class IndexModel : PageModel
    {
        public ProductCategorySearchModel? SearchModel;
        public List<ProductCategoryViewModel>? ProductCategories;

        private readonly IProductCategoryApplication _productCategoryApplication;

        public IndexModel(IProductCategoryApplication productCategoryApplication)
        {
            _productCategoryApplication = productCategoryApplication;
        }

        //[NeedsPermission(ShopPermissions.ListProductCategories)]
        public async Task OnGet(ProductCategorySearchModel searchModel, CancellationToken cancellation)
        {
            ProductCategories = await _productCategoryApplication.Search(searchModel, cancellation);
        }

        public async Task<IActionResult> OnGetCreate(CancellationToken cancellation)
        {
            var command = new CreateProductCategory
            {
                Categories = await _productCategoryApplication.GetProductCategories(cancellation)
            };
            return Partial("./Create", command);
        }

        //[NeedsPermission(ShopPermissions.CreateProductCategory)]
        public async Task<JsonResult> OnPostCreate(CreateProductCategory command, CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return new JsonResult(new { IsSucceeded = false, Message = "ورودی نامعتبر است." });

            var result = await _productCategoryApplication.Create(command, cancellation);

            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetEdit(long id, CancellationToken cancellation)
        {
            var productCategory = await _productCategoryApplication.GetDetails(id, cancellation);
            productCategory.Categories = (await _productCategoryApplication.GetProductCategories(cancellation))
                .Where(x => x.Id != id)
                .ToList();

            return Partial("Edit", productCategory);
        }

        //[NeedsPermission(ShopPermissions.EditProductCategory)]
        public async Task<JsonResult> OnPostEdit(EditProductCategory command, CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return new JsonResult(new { IsSucceeded = false, Message = "ورودی نامعتبر است." });


            var result = await _productCategoryApplication.Edit(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }
    }
}