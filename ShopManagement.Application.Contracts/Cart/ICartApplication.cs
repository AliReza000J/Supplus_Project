using _0_Framework.Application;

namespace ShopManagement.Application.Contracts.Cart
{
    public interface ICartApplication
    {
        Task<OperationResult> CreateCartAsync(string userId);
        Task<OperationResult> ClearCartItemsAsync(string cartKey);
        Task<OperationResult> AddOrIncreaseCartItem(AddToCart command);
        Task<OperationResult> UpdateCartItemAsync(UpdateCartItem command);
        Task<OperationResult> RemoveFromCartAsync(RemoveFromCart command);
        Task<OperationResult> SyncAnonymousCartToUserAsync(string anonCartKey, long userId);
    }
}