using _0_Framework.Application;

namespace BlogManagement.Application.Contracts.Article
{
    public interface IArticleApplication
    {
        Task<OperationResult> Create(CreateArticle command);
        Task<OperationResult> Edit(EditArticle command);
        Task<OperationResult> IncrementArticleViewCountAsync(long articleId, string? userId = null);
        Task<EditArticle> GetDetails(long id);
        Task<List<ArticleViewModel>> Search(ArticleSearchModel searchModel);
    }
}
