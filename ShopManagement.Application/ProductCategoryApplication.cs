using _0_Framework.Application;
using ShopManagement.Application.Contracts.ProductCategory;
using ShopManagement.Domain.ProductCategoryAgg;

namespace ShopManagement.Application
{
    public class ProductCategoryApplication : IProductCategoryApplication
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IFileUploader _fileUploader;

        public ProductCategoryApplication(IProductCategoryRepository productCategoryRepository, IFileUploader fileUploader)
        {
            _productCategoryRepository = productCategoryRepository;
            _fileUploader = fileUploader;
        }
        public async Task<OperationResult> Create(CreateProductCategory command)
        {
            var operation = new OperationResult();

            if (await _productCategoryRepository.ExistsAsync
                (x => x.Name == command.Name && x.ParentId == command.ParentId))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            if (command.ParentId.HasValue)
            {
                var parent = await _productCategoryRepository.GetAsync(command.ParentId.Value);
                if (parent == null)
                    return operation.Failed("گروه والد یافت نشد.");

            }

            var slug = command.Slug.Slugify();
            var fullSlug = await GetFullSlug(command.ParentId, slug);

            var picturePath = $"ProductCategories/{fullSlug}";
            var pictureName = _fileUploader.Upload(command.Picture, picturePath);

            var productCategory = new ProductCategory(
                command.Name, command.Description, pictureName, command.PictureTitle,
                command.PictureAlt, slug, fullSlug, command.MetaDescription, command.Keywords, command.ParentId);

            _productCategoryRepository.Create(productCategory);

            await _productCategoryRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        private async Task<string> GetFullSlug(long? parentId, string slug)
        {
            if (!parentId.HasValue)
                return slug;

            var parent = await _productCategoryRepository.GetAsync(parentId.Value);
            if (parent == null)
                return slug;

            return $"{parent.FullSlug}/{slug}";
        }

        public async Task<OperationResult> Edit(EditProductCategory command)
        {
            var operation = new OperationResult();
            var productCategory = await _productCategoryRepository.GetWithChildren(command.Id);

            if (productCategory == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _productCategoryRepository.ExistsAsync
                (x => x.Name == command.Name && x.ParentId == command.ParentId && x.Id != command.Id))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            // Check for cycle: if command.ParentId is descendant of category.Id -> invalid
            if (command.ParentId.HasValue)
            {
                if (command.ParentId.Value == command.Id)
                    return operation.Failed("دسته نمی‌تواند والد خودش باشد.");

                var parent = await _productCategoryRepository.GetAsync(command.ParentId.Value);
                if (parent == null)
                    return operation.Failed("گروه والد یافت نشد.");

                if (parent.IsDescendantOf(productCategory.Id))
                    return operation.Failed("نمی‌توانید دسته را به فرزند خودش منتقل کنید (ایجاد حلقه).");
            }

            var slug = command.Slug.Slugify();
            var fullSlug = await GetFullSlug(command.ParentId, slug);

            var picturePath = $"ProductCategories/{fullSlug}";
            var pictureName = _fileUploader.Upload(command.Picture, picturePath);

            if (command.Picture != null)
            {
                pictureName = _fileUploader.Upload(command.Picture, picturePath);
            }

            productCategory.Edit(command.Name, command.Description, pictureName, command.PictureTitle,
                command.PictureAlt, slug, fullSlug, command.MetaDescription, command.Keywords, command.ParentId);

            UpdateChildrenFullSlugs(productCategory); //Update

            await _productCategoryRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        private static void UpdateChildrenFullSlugs(ProductCategory parent)
        {
            if (parent.Children == null || !parent.Children.Any())
                return;

            foreach (var child in parent.Children)
            {
                child.SetFullSlug($"{parent.FullSlug}/{child.Slug}");
                UpdateChildrenFullSlugs(child);
            }
        }

        public async Task<EditProductCategory?> GetDetails(long id)
        {
            return await _productCategoryRepository.GetDetails(id);
        }

        public async Task<List<ProductCategoryViewModel>> GetProductCategories()
        {
            return await _productCategoryRepository.GetProductCategories();
        }

        public async Task<List<ProductCategoryViewModel>> Search(ProductCategorySearchModel searchModel)
        {
            return await _productCategoryRepository.Search(searchModel);
        }

        public async Task<List<ProductCategoryViewModel>> GetLeafCategories()
        {
            return await _productCategoryRepository.GetLeafCategories();
        }
    }
}
