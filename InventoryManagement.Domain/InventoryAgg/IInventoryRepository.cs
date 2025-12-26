using _0_Framework.Domain;
using InventoryManagement.Application.Contracts.Inventory;

namespace InventoryManagement.Domain.InventoryAgg
{
    public interface IInventoryRepository : IRepository<long, Inventory>
    {
        Task<EditInventory> GetDetails(long id);
        Task<Inventory?> GetBy(long productId);
        Task<List<InventoryViewModel>> Search(InventorySearchModel searchModel);
        Task<List<InventoryOperationViewModel>> GetOperationLog(long inventoryId);
    }
}
