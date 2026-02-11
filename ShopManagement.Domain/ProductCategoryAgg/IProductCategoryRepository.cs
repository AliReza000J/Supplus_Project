using _0_Framework.Domain;
using ShopManagement.Application.Contracts.ProductCategory;

namespace ShopManagement.Domain.ProductCategoryAgg
{
    public interface IProductCategoryRepository: IRepository<long, ProductCategory>
    {
        Task<List<ProductCategoryViewModel>> GetProductCategories(CancellationToken cancellation); //with parent for admin panel
        Task<EditProductCategory?> GetDetails(long id, CancellationToken cancellation);
        Task<string?> GetFullSlugById(long id, CancellationToken cancellation);
        Task<List<ProductCategoryViewModel>> Search(ProductCategorySearchModel searchModel, CancellationToken cancellation);

        Task<ProductCategory?> GetWithChildren(long id, CancellationToken cancellation);

        Task<List<ProductCategoryViewModel>> GetLeafCategories(CancellationToken cancellation);
    }
}
