using _0_Framework.Application;

namespace _01_PharmaPlusQuery.Contracts.Article
{
    public interface IArticleQuery
    {
        Task<List<ArticleQueryModel>> GetTopRatedArticles(int take = 10, CancellationToken cancellation = default);
        Task<List<ArticleQueryModel>> GetMostViewedArticles(int take = 10, CancellationToken cancellation = default);
        Task<List<ArticleQueryModel>> GetLatestArticles(CancellationToken cancellation);
        Task<ArticleQueryModel> GetArticleDetails(string slug, CancellationToken cancellation);
        Task<PagedResult<ArticleQueryModel>> Search(string value, int page, int pageSize, CancellationToken cancellation);
        Task<long> GetArticleIdBySlug(string articleSlug, CancellationToken cancellation);
    }
}
