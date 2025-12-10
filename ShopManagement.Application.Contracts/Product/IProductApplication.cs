using _0_Framework.Application;

namespace ShopManagement.Application.Contracts.Product
{
    public interface IProductApplication
    {
        Task<OperationResult> Create(CreateProduct command);
        Task<OperationResult> Edit(EditProduct command);
        Task<OperationResult> IncrementProductViewCountAsync(long productId, string? userId = null);
        Task<OperationResult> AddFavoriteAsync(string userId, long productId);
        Task<OperationResult> DeleteFavoriteAsync(string userId, long productId);
        Task<EditProduct?> GetDetails(long id);
        Task<List<ProductViewModel>> GetProducts();
        Task<List<ProductViewModel>> Search(ProductSearchModel searchModel);
    }
}
