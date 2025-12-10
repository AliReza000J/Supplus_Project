using _0_Framework.Domain;
using ShopManagement.Application.Contracts.ProductCategory;

namespace ShopManagement.Domain.ProductCategoryAgg
{
    public interface IProductCategoryRepository: IRepository<long, ProductCategory>
    {
        Task<List<ProductCategoryViewModel>> GetProductCategories(); //with parent for admin panel
        Task<EditProductCategory?> GetDetails(long id);
        Task<string?> GetFullSlugById(long id);
        Task<List<ProductCategoryViewModel>> Search(ProductCategorySearchModel searchModel);

        Task<ProductCategory?> GetWithChildren(long id);

        Task<List<ProductCategoryViewModel>> GetLeafCategories();
    }
}
