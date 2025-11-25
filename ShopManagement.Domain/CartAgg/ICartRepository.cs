using _0_Framework.Domain;

namespace ShopManagement.Domain.CartAgg
{
    public interface ICartRepository: IRepository<long, Cart>
    {
        Task<Cart?> GetByCartKeyAsync(string cartKey);
        Task CreateAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task DeleteAsync(Cart cart);
    }
}