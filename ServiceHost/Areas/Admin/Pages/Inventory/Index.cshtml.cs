using _0_Framework.Infrastructure;
using InventoryManagement.Application.Contracts.Inventory;
using InventoryManagement.Infrastructure.Configuration.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopManagement.Application.Contracts.Product;

namespace ServiceHost.Areas.Admin.Pages.Inventory
{
    //[Authorize(Roles = Roles.Administrator)]
    public class IndexModel : PageModel
    {
        [TempData] public string Message { get; set; }
        public InventorySearchModel SearchModel;
        public List<InventoryViewModel> Inventory;
        public SelectList Products;

        private readonly IProductApplication _productApplication;
        private readonly IInventoryApplication _inventoryApplication;

        public IndexModel(IProductApplication productApplication, IInventoryApplication inventoryApplication)
        {
            _productApplication = productApplication;
            _inventoryApplication = inventoryApplication;
        }

        [NeedsPermission(InventoryPermissions.ListInventory)]
        public async Task OnGet(InventorySearchModel searchModel)
        {
            Products = new SelectList(await _productApplication.GetProducts(), "Id", "Name");
            Inventory = await _inventoryApplication.Search(searchModel);
        }

        [NeedsPermission(InventoryPermissions.CreateInventory)]
        public async Task<IActionResult> OnGetCreate()
        {
            var command = new CreateInventory
            {
                Products = await _productApplication.GetProducts()
            };
            return Partial("./Create", command);
        }

        [NeedsPermission(InventoryPermissions.CreateInventory)]
        public async Task<JsonResult> OnPostCreate(CreateInventory command)
        {
            var result = await _inventoryApplication.Create(command);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetEdit(long id)
        {
            var inventory = await _inventoryApplication.GetDetails(id);
            inventory.Products = await _productApplication.GetProducts();
            return Partial("Edit", inventory);
        }

        [NeedsPermission(InventoryPermissions.EditInventory)]
        public async Task<JsonResult> OnPostEdit(EditInventory command)
        {
            var result = await _inventoryApplication.Edit(command);
            return new JsonResult(result);
        }

        public IActionResult OnGetIncrease(long id)
        {
            var command = new IncreaseInventory()
            {
                InventoryId = id
            };
            return Partial("Increase", command);
        }

        [NeedsPermission(InventoryPermissions.Increase)]
        public async Task<JsonResult> OnPostIncrease(IncreaseInventory command)
        {
            var result = await _inventoryApplication.Increase(command);
            return new JsonResult(result);
        }

        public IActionResult OnGetReduce(long id)
        {
            var command = new ReduceInventory()
            {
                InventoryId = id
            };
            return Partial("Reduce", command);
        }

        [NeedsPermission(InventoryPermissions.Reduce)]
        public async Task<JsonResult> OnPostReduce(ReduceInventory command)
        {
            var result = await _inventoryApplication.Reduce(command);
            return new JsonResult(result);
        }

        [NeedsPermission(InventoryPermissions.OperationLog)]
        public async Task<IActionResult> OnGetLog(long id)
        {
            var log = await _inventoryApplication.GetOperationLog(id);
            return Partial("OperationLog", log);
        }
    }
}