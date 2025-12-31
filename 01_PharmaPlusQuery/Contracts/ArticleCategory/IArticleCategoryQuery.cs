namespace _01_PharmaPlusQuery.Contracts.ArticleCategory
{
    public interface IArticleCategoryQuery
    {
        Task<ArticleCategoryQueryModel> GetArticleCategory(string slug, CancellationToken cancellation);
        Task<List<ArticleCategoryQueryModel>> GetArticleCategories(CancellationToken cancellation);
    }
}
