using _0_Framework.Domain;
using ShopManagement.Application.Contracts.ProductCategory;

namespace ShopManagement.Domain.ProductCategoryAgg
{
    public interface IProductCategoryRepository: IRepository<long, ProductCategory>
    {
        List<ProductCategoryViewModel> GetProductCategories(); //with parent
        EditProductCategory GetDetails(long id);
        string GetSlugById(long id);
        List<ProductCategoryViewModel> Search(ProductCategorySearchModel searchModel);

        // queries for tree
        //List<ProductCategoryViewModel> GetAllWithChildren();
        List<ProductCategoryViewModel> GetLeafCategories();
    }
}
