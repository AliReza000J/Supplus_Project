using _0_Framework.Domain;

namespace ShopManagement.Domain.CartAgg
{
    public interface ICartRepository: IRepository<long, Cart>
    {
        Task<Cart?> GetByCartKeyAsync(string cartKey);
        Task UpdateAsync(Cart cart);
        void DeleteCartItem(CartItem cartItem);
    }
}