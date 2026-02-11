using _0_Framework.Application;

namespace ShopManagement.Application.Contracts.ProductCategory
{
    public interface IProductCategoryApplication
    {
        Task<OperationResult> Create(CreateProductCategory command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditProductCategory command, CancellationToken cancellation);
        Task<EditProductCategory?> GetDetails(long id, CancellationToken cancellation);
        Task<List<ProductCategoryViewModel>> Search(ProductCategorySearchModel searchModel, CancellationToken cancellation);
        Task<List<ProductCategoryViewModel>> GetProductCategories(CancellationToken cancellation);
        Task<List<ProductCategoryViewModel>> GetLeafCategories(CancellationToken cancellation);
    }
}
