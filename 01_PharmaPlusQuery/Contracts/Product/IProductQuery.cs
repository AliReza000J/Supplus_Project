using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.DTOs;

namespace _01_PharmaPlusQuery.Contracts.Product
{
    public interface IProductQuery
    {
        Task<ProductQueryModel?> GetProductDetails(string productSlug, long userId, CancellationToken cancellationToken = default);
        Task<List<ProductCardModel>> GetLatestArrivals(CancellationToken cancellationToken);
        Task<List<ProductCardModel>> GetTopRatedProducts(int take = 10, CancellationToken cancellationToken = default);
        Task<List<ProductCardModel>> GetMostViewedProducts(int take = 10, CancellationToken cancellationToken = default);
        Task<List<ProductCardModel>> GetRandomRecommendations(int take = 10, CancellationToken cancellationToken = default);
        Task<PagedResult<ProductCardModel>> Search(ProductFilterRequest request, CancellationToken cancellationToken = default);
        //Task<SearchByBrandResult?> GetProductsByBrand(string brandName, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<List<FaveProductsCardModel>> GetUserFavoriteList(long userId, CancellationToken cancellationToken = default);
        Task<bool> Exists(long productId, CancellationToken cancellationToken = default);
    }
}
