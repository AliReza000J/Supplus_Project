namespace ShopManagement.Domain.Services
{
    public interface IInventoryService
    {
        Task<int> GetAvailableQuantityAsync(long productId);
        /// <summary>
        /// Try reduce inventory; return true if success
        /// </summary>
        Task<bool> ReduceAsync(long productId, int quantity);
        /// <summary>
        /// Increase / rollback inventory
        /// </summary>
        Task IncreaseAsync(long productId, int quantity);
    }
}
