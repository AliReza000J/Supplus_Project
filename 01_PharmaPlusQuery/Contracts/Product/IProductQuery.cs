using _0_Framework.Application;

namespace _01_PharmaPlusQuery.Contracts.Product
{
    public interface IProductQuery
    {
        Task<ProductQueryModel> GetProductDetails(string productSlug);
        Task<List<ProductQueryModel>> GetLatestArrivals();
        Task<PagedResult<ProductQueryModel>> Search(string value, int page, int pageSize);
        //List<CartItem> CheckInventoryStatus(List<CartItem> cartItems);
    }
}
