using _0_Framework.Domain;
using ShopManagement.Application.Contracts.ProductCategory;

namespace ShopManagement.Domain.ProductCategoryAgg
{
    public interface IProductCategoryRepository: IRepository<long, ProductCategory>
    {
        List<ProductCategoryViewModel> GetProductCategories(); //with parent
        EditProductCategory GetDetails(long id);
        string GetFullSlugById(long id);
        List<ProductCategoryViewModel> Search(ProductCategorySearchModel searchModel);

        public ProductCategory GetWithChildren(long id);

        // queries for tree
        //List<ProductCategoryViewModel> GetAllWithChildren();
        List<ProductCategoryViewModel> GetLeafCategories();
    }
}
