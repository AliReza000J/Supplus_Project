using _0_Framework.Domain;
using BlogManagement.Application.Contracts.Article;

namespace BlogManagement.Domain.ArticleAgg
{
    public interface IArticleRepository : IRepository<long, Article>
    {
        Task<EditArticle?> GetDetails(long id, CancellationToken cancellation);
        Task<Article> GetWithCategory(long id, CancellationToken cancellation);
        Task<List<ArticleViewModel>> Search(ArticleSearchModel searchModel, CancellationToken cancellation);
    }
}
