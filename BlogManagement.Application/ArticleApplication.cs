using _0_Framework.Application;
using BlogManagement.Application.Contracts.Article;
using BlogManagement.Domain.ArticleAgg;
using BlogManagement.Domain.ArticleCategoryAgg;
using BlogManagement.Domain.UserArticleInteractionAgg;

namespace BlogManagement.Application
{
    public class ArticleApplication : IArticleApplication
    {
        private readonly IFileUploader _fileUploader;
        private readonly IArticleRepository _articleRepository;
        private readonly IArticleCategoryRepository _articleCategoryRepository;
        private readonly IArticleViewRepository _articleViewRepository;
        public ArticleApplication(IArticleRepository articleRepository, IFileUploader fileUploader,
            IArticleCategoryRepository articleCategoryRepository, IArticleViewRepository articleViewRepository)
        {
            _fileUploader = fileUploader;
            _articleRepository = articleRepository;
            _articleCategoryRepository = articleCategoryRepository;
            _articleViewRepository = articleViewRepository;
        }

        public async Task<OperationResult> Create(CreateArticle command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            if (await _articleRepository.ExistsAsync(x => x.Title == command.Title, cancellation))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var slug = command.Slug.Slugify();
            var categorySlug = await _articleCategoryRepository.GetSlugBy(command.CategoryId, cancellation);
            var path = $"ArticleCategories/{categorySlug}/Articles/{slug}";
            var pictureName = await _fileUploader.UploadAsync(command.Picture, path, cancellation);
            var publishDate = command.PublishDate.ToGeorgianDateTime();

            var article = new Article(command.Title, command.ShortDescription, command.Body, pictureName,
                command.PictureAlt, command.PictureTitle, publishDate, slug, command.Keywords, command.MetaDescription,
                command.CanonicalAddress, command.CategoryId);

            _articleRepository.Create(article);
            await _articleRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditArticle command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var article = await _articleRepository.GetWithCategory(command.Id, cancellation);

            if (article == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _articleRepository.ExistsAsync(x => x.Title == command.Title && x.Id != command.Id, cancellation))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var slug = command.Slug.Slugify();
            var path = $"ArticleCategories/{article.Category.Slug}/Articles/{slug}";
            var pictureName = "";

            if (!(command.Picture == null && string.IsNullOrEmpty(command.Picture?.FileName)))
            {
                _fileUploader.DeleteFile(article.Picture);
                pictureName = await _fileUploader.UploadAsync(command.Picture, path, cancellation);
            }

            var publishDate = command.PublishDate.ToGeorgianDateTime();

            article.Edit(command.Title, command.ShortDescription, command.Body, pictureName,
                command.PictureAlt, command.PictureTitle, publishDate, slug, command.Keywords, command.MetaDescription,
                command.CanonicalAddress, command.CategoryId);

            await _articleRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<EditArticle?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _articleRepository.GetDetails(id, cancellation);
        }

        public async Task<OperationResult> IncrementArticleViewCountAsync(long articleId, long userId = 0, CancellationToken cancellation = default)
        {
            var op = new OperationResult();
            var article = await _articleRepository.GetAsync(articleId, cancellation);
            if (article == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            article.AddView();
            _articleViewRepository.Create(new ArticleView(articleId, userId));

            await _articleViewRepository.SaveChangesAsync(cancellation);
            return op.Succeeded();
        }

        public async Task<List<ArticleViewModel>> Search(ArticleSearchModel searchModel, CancellationToken cancellation)
        {
            return await _articleRepository.Search(searchModel, cancellation);
        }
    }
}
