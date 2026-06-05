using _0_Framework.Domain;
using ShopManagement.Application.Contracts.Product;

namespace ShopManagement.Domain.ProductAgg
{
    public interface IProductRepository : IRepository<long, Product>
    {
        Task<string?> GetProductName(long prodcutId, CancellationToken cancellation);
        Task<string?> GetProductPic(long prodcutId, CancellationToken cancellation);
        Task<List<string>?> GetDistinctBrands(CancellationToken cancellationToken);
        Task<EditProduct?> GetDetails(long id, CancellationToken cancellation);
        Task<Product?> GetProductWithCategory(long id, CancellationToken cancellation);
        Task<List<Product>> GetProductsWithCategoryByIdsAsync(List<long> productIds, CancellationToken cancellation);
        Task<List<(long ProductId, long CategoryId, string Brand, string Name, string Picture)>> GetProductDetailsByIdsAsync(List<long> productIds, CancellationToken cancellation);
        Task<List<ProductViewModel>> GetProducts(CancellationToken cancellation);
        Task<List<ProductViewModel>> Search(ProductSearchModel searchModel, CancellationToken cancellation);
    }
}