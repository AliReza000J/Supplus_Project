namespace _01_PharmaPlusQuery.Contracts.Cart
{
    public interface ICartQuery
    {
        Task<CartViewModel> GetCartAsync(string cartKey, CancellationToken cancellation);
    }
}