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
        public OperationResult Create(CreateProductCategory command)
        {
            var operation = new OperationResult();

            if (_productCategoryRepository.Exists(x => x.Name == command.Name && x.ParentId == command.ParentId))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            if (command.ParentId.HasValue)
            {
                var parent = _productCategoryRepository.Get(command.ParentId.Value);
                if (parent == null)
                    return operation.Failed("گروه والد یافت نشد.");

            }

            var slug = command.Slug.Slugify();
            var fullSlug = GetFullSlug(command.ParentId, slug);

            var picturePath = $"ProductCategories/{fullSlug}";
            var pictureName = _fileUploader.Upload(command.Picture, picturePath);

            var productCategory = new ProductCategory(
                command.Name, command.Description, pictureName, command.PictureTitle,
                command.PictureAlt, slug, fullSlug, command.MetaDescription, command.Keywords, command.ParentId);

            _productCategoryRepository.Create(productCategory);

            _productCategoryRepository.SaveChanges();
            return operation.Succeeded();
        }

        private string GetFullSlug(long? parentId, string slug)
        {
            if (!parentId.HasValue)
                return slug;

            var parent = _productCategoryRepository.Get(parentId.Value);
            if (parent == null)
                return slug;

            return $"{parent.FullSlug}/{slug}";
        }

        public OperationResult Edit(EditProductCategory command)
        {
            var operation = new OperationResult();
            var productCategory = _productCategoryRepository.GetWithChildren(command.Id);

            if (productCategory == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (_productCategoryRepository.Exists(x => x.Name == command.Name && x.ParentId == command.ParentId && x.Id != command.Id))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            // Check for cycle: if command.ParentId is descendant of category.Id -> invalid
            if (command.ParentId.HasValue)
            {
                if (command.ParentId.Value == command.Id)
                    return operation.Failed("دسته نمی‌تواند والد خودش باشد.");

                var parent = _productCategoryRepository.Get(command.ParentId.Value);
                if (parent == null)
                    return operation.Failed("گروه والد یافت نشد.");

                if (parent.IsDescendantOf(productCategory.Id))
                    return operation.Failed("نمی‌توانید دسته را به فرزند خودش منتقل کنید (ایجاد حلقه).");
            }

            var slug = command.Slug.Slugify();
            var fullSlug = GetFullSlug(command.ParentId, slug);

            var picturePath = $"ProductCategories/{fullSlug}";
            var pictureName = _fileUploader.Upload(command.Picture, picturePath);

            if (command.Picture != null)
            {
                pictureName = _fileUploader.Upload(command.Picture, picturePath);
            }

            productCategory.Edit(command.Name, command.Description, pictureName, command.PictureTitle,
                command.PictureAlt, slug, fullSlug, command.MetaDescription, command.Keywords, command.ParentId);

            UpdateChildrenFullSlugs(productCategory); //Update

            _productCategoryRepository.SaveChanges();
            return operation.Succeeded();
        }

        private void UpdateChildrenFullSlugs(ProductCategory parent)
        {
            if (parent.Children == null || !parent.Children.Any())
                return;

            foreach (var child in parent.Children)
            {
                child.SetFullSlug($"{parent.FullSlug}/{child.Slug}");
                UpdateChildrenFullSlugs(child);
            }
        }

        public EditProductCategory GetDetails(long id)
        {
            return _productCategoryRepository.GetDetails(id);
        }

        public List<ProductCategoryViewModel> GetProductCategories()
        {
            return _productCategoryRepository.GetProductCategories();
        }

        public List<ProductCategoryViewModel> Search(ProductCategorySearchModel searchModel)
        {
            return _productCategoryRepository.Search(searchModel);
        }

        public List<ProductCategoryViewModel> GetLeafCategories()
        {
            return _productCategoryRepository.GetLeafCategories();
        }
    }
}
