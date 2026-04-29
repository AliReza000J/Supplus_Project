namespace _01_PharmaPlusQuery.Contracts.ProductCategory
{
    public interface IProductCategoryQuery
    {
        //Task<ProductCategoryQueryModel> GetLeafProductCategoryWithProductsBy(string slug);
        //Task<List<ProductCategoryQueryModel>> GetProductCategories();
        //Task<List<ProductCategoryQueryModel>> GetProductCategoriesWithProducts();
        Task<ProductCategoryQueryModel?> GetCategoryByFullSlug(string fullSlug, CancellationToken cancellation = default);
        Task<List<ProductCategoryQueryModel>> GetAll(CancellationToken cancellation); //Tree
        //Task<List<ProductCategoryQueryModel>> GetLeafProductCategories(); // last-level categories only
    }
}