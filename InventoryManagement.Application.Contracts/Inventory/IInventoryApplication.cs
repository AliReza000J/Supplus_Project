using _0_Framework.Application;

namespace InventoryManagement.Application.Contracts.Inventory
{
    public interface IInventoryApplication
    {
        Task<OperationResult> Create(CreateInventory command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditInventory command, CancellationToken cancellation);
        Task<OperationResult> Increase(IncreaseInventory command, CancellationToken cancellation);
        Task<OperationResult> ReduceByAdmin(ReduceInventory command, CancellationToken cancellation);
        Task<OperationResult> ReduceByOrder(ReduceInventoryByOrder command, CancellationToken cancellation);
        Task<EditInventory?> GetDetails(long id, CancellationToken cancellation);
        Task<List<InventoryViewModel>> Search(InventorySearchModel searchModel, CancellationToken cancellation);
        Task<List<InventoryOperationViewModel>> GetOperationLog(long inventoryId, CancellationToken cancellation);
    }
}
