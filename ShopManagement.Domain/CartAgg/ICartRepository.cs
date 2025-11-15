namespace ShopManagement.Domain.CartAgg
{
    public interface ICartRepository
    {
        Task<Cart?> GetByCartKeyAsync(string cartKey);
        Task CreateAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task DeleteAsync(Cart cart);
    }
}