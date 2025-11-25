using _0_Framework.Application;

namespace _01_PharmaPlusQuery.Contracts.Article
{
    public interface IArticleQuery
    {
        Task<List<ArticleQueryModel>> LatestArticles();
        Task<ArticleQueryModel> GetArticleDetails(string slug);
        Task<PagedResult<ArticleQueryModel>> Search(string value, int page, int pageSize);
    }
}
