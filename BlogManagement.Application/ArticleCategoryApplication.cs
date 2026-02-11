using _0_Framework.Application;
using BlogManagement.Application.Contracts.ArticleCategory;
using BlogManagement.Domain.ArticleAgg;
using BlogManagement.Domain.ArticleCategoryAgg;
using System.IO;

namespace BlogManagement.Application
{
    public class ArticleCategoryApplication : IArticleCategoryApplication
    {
        private readonly IFileUploader _fileUploader;
        private readonly IArticleCategoryRepository _articleCategoryRepository;

        public ArticleCategoryApplication(IArticleCategoryRepository articleCategoryRepository, IFileUploader fileUploader)
        {
            _fileUploader = fileUploader;
            _articleCategoryRepository = articleCategoryRepository;
        }

        public async Task<OperationResult> Create(CreateArticleCategory command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            if (await _articleCategoryRepository.ExistsAsync(x => x.Name == command.Name, cancellation))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var slug = command.Slug.Slugify();
            var picturePath = $"ArticleCategories/{slug}";
            var pictureName = await _fileUploader.UploadAsync(command.Picture, picturePath, cancellation);
            var articleCategory = new ArticleCategory(command.Name, pictureName, command.PictureAlt, command.PictureTitle
                , command.Description, command.ShowOrder, slug, command.Keywords, command.MetaDescription,
                command.CanonicalAddress);

            _articleCategoryRepository.Create(articleCategory);
            await _articleCategoryRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditArticleCategory command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var articleCategory = await _articleCategoryRepository.GetAsync(command.Id, cancellation);

            if (articleCategory == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _articleCategoryRepository.ExistsAsync(x => x.Name == command.Name && x.Id != command.Id, cancellation))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var slug = command.Slug.Slugify();
            var picturePath = $"ArticleCategories/{slug}";
            var pictureName = "";

            if (!(command.Picture == null && string.IsNullOrEmpty(command.Picture?.FileName)))
            {
                _fileUploader.DeleteFile(articleCategory.Picture);
                pictureName = await _fileUploader.UploadAsync(command.Picture, picturePath, cancellation);
            }

            articleCategory.Edit(command.Name, pictureName, command.PictureAlt, command.PictureTitle,
                command.Description, command.ShowOrder, slug, command.Keywords, command.MetaDescription,
                command.CanonicalAddress);

            await _articleCategoryRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<List<ArticleCategoryViewModel>> GetArticleCategories(CancellationToken cancellation)
        {
            return await _articleCategoryRepository.GetArticleCategories(cancellation);
        }

        public async Task<EditArticleCategory?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _articleCategoryRepository.GetDetails(id, cancellation);
        }

        public async Task<List<ArticleCategoryViewModel>> Search(ArticleCategorySearchModel searchModel, CancellationToken cancellation)
        {
            return await _articleCategoryRepository.Search(searchModel, cancellation);
        }
    }
}
