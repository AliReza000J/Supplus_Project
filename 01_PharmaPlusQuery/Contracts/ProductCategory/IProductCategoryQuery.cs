namespace _01_PharmaPlusQuery.Contracts.ProductCategory
{
    public interface IProductCategoryQuery
    {
        Task<ProductCategoryQueryModel> GetProductCategoryWithProductsBy(string slug);
        Task<List<ProductCategoryQueryModel>> GetProductCategories();
        Task<List<ProductCategoryQueryModel>> GetProductCategoriesWithProducts();
        Task<List<ProductCategoryQueryModel>> GetProductCategoriesTree();
        Task<List<ProductCategoryQueryModel>> GetLeafProductCategories(); // last-level categories only
    }
}