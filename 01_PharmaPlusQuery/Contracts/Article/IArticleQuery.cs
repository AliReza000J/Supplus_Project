using _01_PharmaPlusQuery.Contracts.Product;

namespace _01_PharmaPlusQuery.Contracts.Article
{
    public interface IArticleQuery
    {
        Task<List<ArticleQueryModel>> LatestArticles();
        Task<ArticleQueryModel> GetArticleDetails(string slug);
        Task<List<ArticleQueryModel>> Search(string value);

    }
}
