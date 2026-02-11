using _0_Framework.Domain;

namespace ShopManagement.Domain.CartAgg
{
    public interface ICartRepository: IRepository<long, Cart>
    {
        Task<Cart?> GetOrCreateByCartKeyAsync(string cartKey, CancellationToken cancellation);
        Task UpdateAsync(Cart cart, CancellationToken cancellation);
        void DeleteCartItem(CartItem cartItem);
    }
}