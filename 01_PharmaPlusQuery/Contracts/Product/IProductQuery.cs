using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.DTOs;

namespace _01_PharmaPlusQuery.Contracts.Product
{
    public interface IProductQuery
    {
        Task<ProductQueryModel?> GetProductDetails(string productSlug, long userId, CancellationToken cancellationToken = default);
        Task<List<ProductQueryModel>> GetLatestArrivals(CancellationToken cancellationToken);
        Task<List<ProductQueryModel>> GetTopRatedProducts(int take = 10, CancellationToken cancellationToken = default);
        Task<List<ProductQueryModel>> GetMostViewedProducts(int take = 10, CancellationToken cancellationToken = default);
        Task<List<ProductQueryModel>> GetRandomRecommendations(int take = 10, CancellationToken cancellationToken = default);
        Task<PagedResult<ProductQueryModel>> Search(string value, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<SearchByBrandResult?> GetProductsByBrand(string brandName, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<List<ProductQueryModel>> GetUserFavoriteList(long userId, CancellationToken cancellationToken = default);
        Task<bool> Exists(long productId, CancellationToken cancellationToken = default);

    }
}
