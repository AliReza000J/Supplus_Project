using _0_Framework.Application;

namespace ShopManagement.Application.Contracts.Cart
{
    public interface ICartApplication
    {
        Task<OperationResult> CreateCartAsync(long userId, CancellationToken cancellation);
        Task<OperationResult> ClearCartItemsAsync(string cartKey, CancellationToken cancellation);
        Task<OperationResult> AddOrIncreaseCartItem(AddToCart command, CancellationToken cancellation);
        Task<OperationResult> IncreaseCartItem(IncOrDecItem command, CancellationToken cancellation);
        Task<OperationResult> DecreaseCartItem(IncOrDecItem command, CancellationToken cancellation);
        Task<OperationResult> UpdateCartItemAsync(UpdateCartItem command, CancellationToken cancellation);
        Task<OperationResult> RemoveFromCartAsync(RemoveFromCart command, CancellationToken cancellation);
        Task<OperationResult> SyncCartAsync(SyncCart command, CancellationToken cancellation);
    }
}