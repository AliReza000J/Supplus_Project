using _0_Framework.Application;

namespace ShopManagement.Application.Contracts.Product
{
    public interface IProductApplication
    {
        Task<OperationResult> Create(CreateProduct command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditProduct command, CancellationToken cancellation);
        Task<OperationResult> IncreaseProductViewCountAsync(long productId, long userId = 0, CancellationToken cancellation = default);
        Task<OperationResult> AddFavoriteAsync(long userId, long productId, CancellationToken cancellation);
        Task<OperationResult> DeleteFavoriteAsync(long userId, long productId, CancellationToken cancellation);
        Task<EditProduct?> GetDetails(long id, CancellationToken cancellation);
        Task<List<ProductViewModel>> GetProducts(CancellationToken cancellation);
        Task<List<ProductViewModel>> Search(ProductSearchModel searchModel, CancellationToken cancellation);
    }
}
