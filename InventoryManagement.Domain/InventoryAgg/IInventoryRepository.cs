using _0_Framework.Domain;
using InventoryManagement.Application.Contracts.Inventory;

namespace InventoryManagement.Domain.InventoryAgg
{
    public interface IInventoryRepository : IRepository<long, Inventory>
    {
        Task<EditInventory?> GetDetails(long id, CancellationToken cancellation);
        Task<Inventory?> GetBy(long productId, CancellationToken cancellation);
        Task<List<Inventory>> GetByProductIds(List<long> productIds, CancellationToken cancellation);
        Task<List<InventoryViewModel>> Search(InventorySearchModel searchModel, CancellationToken cancellation);
        Task<List<InventoryOperationViewModel>> GetOperationLog(long inventoryId, CancellationToken cancellation);
    }
}
