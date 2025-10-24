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
                if (parent != null && parent.IsDescendantOf(command.ParentId.Value))
                    return operation.Failed("انتخاب والد باعث ایجاد حلقه می‌شود.");
            }

            var slug = command.Slug.Slugify();

            string fullPath = GetCategoryFullPath(command.ParentId, slug);

            var pictureName = _fileUploader.Upload(command.Picture, fullPath);

            var productCategory = new ProductCategory(
                command.Name, command.Description, pictureName, command.PictureTitle,
                command.PictureAlt, slug, command.MetaDescription, command.Keywords, command.ParentId);

            _productCategoryRepository.Create(productCategory);

            _productCategoryRepository.SaveChanges();
            return operation.Succeeded();
        }

        private string GetCategoryFullPath(long? parentId, string currentSlug)
        {
            if (!parentId.HasValue)
                return $"ProductCategories/{currentSlug}";

            var parent = _productCategoryRepository.Get(parentId.Value);
            if (parent == null)
                return $"ProductCategories/{currentSlug}";

            var parentPath = GetCategoryFullPath(parent.ParentId, parent.Slug);
            return $"{parentPath}/{currentSlug}";
        }

        public OperationResult Edit(EditProductCategory command)
        {
            var operation = new OperationResult();
            var productCategory = _productCategoryRepository.Get(command.Id);

            if (productCategory == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (_productCategoryRepository.Exists(x => x.Name == command.Name && x.ParentId == command.ParentId && x.Id != command.Id))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            // Check for cycle: if command.ParentId is descendant of category.Id -> invalid
            if (command.ParentId.HasValue)
            {
                var parent = _productCategoryRepository.Get(command.ParentId.Value);
                if (parent != null && parent.IsDescendantOf(command.Id))
                    return operation.Failed("انتخاب والد باعث ایجاد حلقه می‌شود.");
            }

            var slug = command.Slug.Slugify();

            string fullPath = GetCategoryFullPath(command.ParentId, slug);
            string pictureName = productCategory.Picture;

            if (command.Picture != null)
            {
                pictureName = _fileUploader.Upload(command.Picture, fullPath);
            }

            productCategory.Edit(command.Name, command.Description, pictureName, command.PictureTitle,
                command.PictureAlt, slug, command.MetaDescription, command.Keywords, command.ParentId);

            _productCategoryRepository.SaveChanges();
            return operation.Succeeded();
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
