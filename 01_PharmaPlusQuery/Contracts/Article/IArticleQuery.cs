using _0_Framework.Application;

namespace _01_PharmaPlusQuery.Contracts.Article
{
    public interface IArticleQuery
    {
        Task<List<ArticleQueryModel>> GetTopRatedArticles(int take = 10);
        Task<List<ArticleQueryModel>> GetMostViewedArticles(int take = 10);
        Task<List<ArticleQueryModel>> GetLatestArticles();
        Task<ArticleQueryModel> GetArticleDetails(string slug);
        Task<PagedResult<ArticleQueryModel>> Search(string value, int page, int pageSize);
        Task<long> GetArticleIdBySlug(string articleSlug);
    }
}
