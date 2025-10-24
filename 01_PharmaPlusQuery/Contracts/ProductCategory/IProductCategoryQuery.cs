namespace _01_PharmaPlusQuery.Contracts.ProductCategory
{
    public interface IProductCategoryQuery
    {
        Task<ProductCategoryQueryModel> GetProductCategoryWithProductsBy(string slug);
        Task<List<ProductCategoryQueryModel>> GetProductCategories();
        Task<List<ProductCategoryQueryModel>> GetProductCategoriesWithProducts();
    }
}