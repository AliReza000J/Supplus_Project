using _0_Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.CartAgg;
using ShopManagement.Domain.ProductAgg;
using System.Globalization;

namespace ShopManagement.Infrastructure.EFCore.Repository
{
    public class CartRepository: BaseRepository<long, Cart>, ICartRepository
    {
        private readonly ShopContext _context;

        public CartRepository(ShopContext context) : base(context)
        {
            _context = context;
        }

        public void DeleteCartItem(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
        }

        public async Task<Cart?> GetByCartKeyAsync(string cartKey)
        {
            return await _context.Carts
                .Include(x=>x.Items)
                .FirstOrDefaultAsync(
                x => x.CartKey == cartKey);
        }

        public Task UpdateAsync(Cart cart)
        {
            _context.Carts.Update(cart);
            return Task.CompletedTask;
        }
    }
}
