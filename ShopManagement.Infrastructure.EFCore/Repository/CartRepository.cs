using _0_Framework.Infrastructure;
using AccountManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.CartAgg;
using System.Text.RegularExpressions;

namespace ShopManagement.Infrastructure.EFCore.Repository
{
    public class CartRepository : RepositoryBase<long, Cart>, ICartRepository
    {
        private readonly ShopContext _context;
        private readonly AccountContext _accContext;

        public CartRepository(ShopContext context, AccountContext accContext) : base(context)
        {
            _context = context;
            this._accContext = accContext;
        }

        public void DeleteCartItem(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
        }

        public async Task<Cart?> GetOrCreateByCartKeyAsync(string cartKey, CancellationToken cancellation)
        {
            var cart = await _context.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(
                    x => x.CartKey == cartKey,
                cancellation);

            var id = long.Parse(cartKey[5..]);
            var isIdValid = await _accContext.Accounts.AnyAsync(x => x.Id == id, cancellation);

            if (cart == null && Regex.IsMatch(cartKey, @"^user:\d+$") && isIdValid)
            {
                cart = Cart.Create(cartKey);
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(cancellation);
            }

            return cart;
        }

        public Task UpdateAsync(Cart cart, CancellationToken cancellation = default)
        {
            _context.Carts.Update(cart);
            return Task.CompletedTask;
        }
    }
}
