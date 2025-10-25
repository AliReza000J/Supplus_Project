namespace _01_PharmaPlusQuery.Contracts.ProductCategory
{
    public interface IProductCategoryQuery
    {
        //Task<ProductCategoryQueryModel> GetLeafProductCategoryWithProductsBy(string slug);
        //Task<List<ProductCategoryQueryModel>> GetProductCategories();
        //Task<List<ProductCategoryQueryModel>> GetProductCategoriesWithProducts();
        Task<ProductCategoryQueryModel> GetCategoryByFullSlug(string fullSlug);
        Task<List<ProductCategoryQueryModel>> GetAll(); //Tree
        //Task<List<ProductCategoryQueryModel>> GetLeafProductCategories(); // last-level categories only
    }
}