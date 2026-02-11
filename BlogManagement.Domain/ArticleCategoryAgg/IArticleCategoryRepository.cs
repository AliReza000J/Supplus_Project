using _0_Framework.Domain;
using BlogManagement.Application.Contracts.ArticleCategory;

namespace BlogManagement.Domain.ArticleCategoryAgg
{
    public interface IArticleCategoryRepository : IRepository<long, ArticleCategory>
    {
        Task<string> GetSlugBy(long id, CancellationToken cancellation);
        Task<EditArticleCategory?> GetDetails(long id, CancellationToken cancellation);
        Task<List<ArticleCategoryViewModel>> GetArticleCategories(CancellationToken cancellation);
        Task<List<ArticleCategoryViewModel>> Search(ArticleCategorySearchModel searchModel, CancellationToken cancellation);
    }
}
