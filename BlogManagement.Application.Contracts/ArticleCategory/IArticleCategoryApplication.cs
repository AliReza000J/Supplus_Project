using _0_Framework.Application;

namespace BlogManagement.Application.Contracts.ArticleCategory
{
    public interface IArticleCategoryApplication
    {
        Task<OperationResult> Create(CreateArticleCategory command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditArticleCategory command, CancellationToken cancellation);
        Task<EditArticleCategory?> GetDetails(long id, CancellationToken cancellation);
        Task<List<ArticleCategoryViewModel>> GetArticleCategories(CancellationToken cancellation);
        Task<List<ArticleCategoryViewModel>> Search(ArticleCategorySearchModel searchModel, CancellationToken cancellation);
    }                                      
}
