using _0_Framework.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopManagement.Application.Contracts.Product;
using ShopManagement.Application.Contracts.ProductCategory;
using ShopManagement.Configuration.Permissions;

namespace ServiceHost.Areas.Admin.Pages.Shop.Products
{
    public class IndexModel : PageModel
    {
        public ProductSearchModel SearchModel;
        public List<ProductViewModel> Products;
        public SelectList ProductCategories;

        private readonly IProductApplication _productApplication;
        private readonly IProductCategoryApplication _productCategoryApplication;

        public IndexModel(IProductApplication productApplication,
            IProductCategoryApplication productCategoryApplication)
        {
            _productApplication = productApplication;
            _productCategoryApplication = productCategoryApplication;
        }

        [NeedsPermission(ShopPermissions.ListProducts)]
        public async Task OnGet(ProductSearchModel searchModel)
        {
            ProductCategories = new SelectList(await _productCategoryApplication.GetLeafCategories(), "Id", "Name");
            Products = await _productApplication.Search(searchModel);
        }
    }
}