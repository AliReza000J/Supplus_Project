using _0_Framework.Application;

namespace BlogManagement.Application.Contracts.Article
{
    public interface IArticleApplication
    {
        Task<OperationResult> Create(CreateArticle command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditArticle command, CancellationToken cancellation);
        Task<OperationResult> IncrementArticleViewCountAsync(long articleId, long userId = 0, CancellationToken cancellation = default);
        Task<EditArticle?> GetDetails(long id, CancellationToken cancellation);
        Task<List<ArticleViewModel>> Search(ArticleSearchModel searchModel, CancellationToken cancellation);
    }
}
