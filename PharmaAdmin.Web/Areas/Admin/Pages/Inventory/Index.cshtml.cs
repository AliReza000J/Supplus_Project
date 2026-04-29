using _0_Framework.Infrastructure;
using InventoryManagement.Application.Contracts.Inventory;
using InventoryManagement.Infrastructure.Configuration.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopManagement.Application.Contracts.Product;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Inventory
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
        public async Task OnGet(InventorySearchModel searchModel, CancellationToken cancellation)
        {
            Products = new SelectList(await _productApplication.GetProducts(cancellation), "Id", "Name");
            Inventory = await _inventoryApplication.Search(searchModel, cancellation);
        }

        [NeedsPermission(InventoryPermissions.CreateInventory)]
        public async Task<IActionResult> OnGetCreate(CancellationToken cancellation)
        {
            var command = new CreateInventory
            {
                Products = await _productApplication.GetProducts(cancellation)
            };
            return Partial("./Create", command);
        }

        [NeedsPermission(InventoryPermissions.CreateInventory)]
        public async Task<JsonResult> OnPostCreate(CreateInventory command, CancellationToken cancellation)
        {
            var result = await _inventoryApplication.Create(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetEdit(long id, CancellationToken cancellation)
        {
            var inventory = await _inventoryApplication.GetDetails(id, cancellation);
            inventory.Products = await _productApplication.GetProducts(cancellation);
            return Partial("Edit", inventory);
        }

        [NeedsPermission(InventoryPermissions.EditInventory)]
        public async Task<JsonResult> OnPostEdit(EditInventory command, CancellationToken cancellation)
        {
            var result = await _inventoryApplication.Edit(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
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
        public async Task<JsonResult> OnPostIncrease(IncreaseInventory command, CancellationToken cancellation)
        {
            var result = await _inventoryApplication.Increase(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
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
        public async Task<JsonResult> OnPostReduce(ReduceInventory command, CancellationToken cancellation)
        {
            var result = await _inventoryApplication.ReduceByAdmin(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        [NeedsPermission(InventoryPermissions.OperationLog)]
        public async Task<IActionResult> OnGetLog(long id, CancellationToken cancellation)
        {
            var log = await _inventoryApplication.GetOperationLog(id, cancellation);
            return Partial("OperationLog", log);
        }
    }
}