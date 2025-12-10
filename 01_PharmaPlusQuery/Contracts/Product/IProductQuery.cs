using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.DTOs;

namespace _01_PharmaPlusQuery.Contracts.Product
{
    public interface IProductQuery
    {
        Task<ProductQueryModel> GetProductDetails(string productSlug);
        Task<List<ProductQueryModel>> GetLatestArrivals();
        Task<List<ProductQueryModel>> GetTopRatedProducts(int take = 10);
        Task<List<ProductQueryModel>> GetMostViewedProducts(int take = 10);
        Task<List<ProductQueryModel>> GetRandomRecommendations(int take = 10);
        Task<PagedResult<ProductQueryModel>> Search(string value, int page, int pageSize);
        Task<SearchByBrandResult> GetProductsByBrand(string brandName, int page, int pageSize);
        Task<List<ProductQueryModel>> GetUserFavoriteList(string accountId);
        Task<bool> Exists(long productId);

        //List<CartItem> CheckInventoryStatus(List<CartItem> cartItems);
    }
}
