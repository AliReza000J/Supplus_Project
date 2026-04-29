using _0_Framework.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopManagement.Application.Contracts.Product;
using ShopManagement.Application.Contracts.ProductPicture;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Shop.ProductPictures
{
    public class IndexModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        public ProductPictureSearchModel SearchModel;
        public List<ProductPictureViewModel> ProductPictures;
        public SelectList Products;

        private readonly IProductApplication _productApplication;
        private readonly IProductPictureApplication _productPictureApplication;
        private readonly ICacheService _cacheService;

        public IndexModel(IProductApplication ProductApplication, IProductPictureApplication productPictureApplication, ICacheService cacheService)
        {
            _productApplication = ProductApplication;
            _productPictureApplication = productPictureApplication;
            _cacheService = cacheService;
        }

        public async Task OnGet(ProductPictureSearchModel searchModel, CancellationToken cancellation)
        {
            Products = new SelectList(await _productApplication.GetProducts(cancellation), "Id", "Name");
            ProductPictures = await _productPictureApplication.Search(searchModel, cancellation);
        }

        public async Task<IActionResult> OnGetCreate(CancellationToken cancellation)
        {
            var command = new CreateProductPicture
            {
                Products = await _productApplication.GetProducts(cancellation)
            };
            return Partial("./Create", command);
        }

        public async Task<JsonResult> OnPostCreate(CreateProductPicture command, CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return new JsonResult(new { IsSucceeded = false, Message = "ورودی نامعتبر است." });

            var result = await _productPictureApplication.Create(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetEdit(long id, CancellationToken cancellation)
        {
            var productPicture = await _productPictureApplication.GetDetails(id, cancellation);
            productPicture.Products = await _productApplication.GetProducts(cancellation);
            return Partial("Edit", productPicture);
        }

        public async Task<JsonResult> OnPostEdit(EditProductPicture command, CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return new JsonResult(new { IsSucceeded = false, Message = "ورودی نامعتبر است." });

            var result = await _productPictureApplication.Edit(command, cancellation);
            _cacheService.Remove($"product:pictures:{command.ProductId}");
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetRemove(long id, CancellationToken cancellation)
        {
            var result = await _productPictureApplication.Remove(id, cancellation);
            var productPicture = await _productPictureApplication.GetDetails(id, cancellation);
            _cacheService.Remove($"product:pictures:{productPicture?.ProductId}");
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetRestore(long id, CancellationToken cancellation)
        {
            var result = await _productPictureApplication.Restore(id, cancellation);
            var productPicture = await _productPictureApplication.GetDetails(id, cancellation);
            _cacheService.Remove($"product:pictures:{productPicture?.ProductId}");
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index");
        }
    }
}
