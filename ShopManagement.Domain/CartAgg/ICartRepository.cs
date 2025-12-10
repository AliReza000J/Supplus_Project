using _0_Framework.Domain;
using ShopManagement.Application.Contracts.Cart;

namespace ShopManagement.Domain.CartAgg
{
    public interface ICartRepository: IRepository<long, Cart>
    {
        Task<Cart?> GetByCartKeyAsync(string cartKey);
        Task UpdateAsync(Cart cart);
        void DeleteCartItem(CartItem cartItem);
    }
}