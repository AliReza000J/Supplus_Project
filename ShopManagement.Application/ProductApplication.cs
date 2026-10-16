using _0_Framework.Application;
using ShopManagement.Application.Contracts.Product;
using ShopManagement.Domain.ProductAgg;
using ShopManagement.Domain.ProductCategoryAgg;
using ShopManagement.Domain.UserProductInteractionAgg;

namespace ShopManagement.Application
{
    public class ProductApplication : IProductApplication
    {
        private readonly IFileUploader _fileUploader;
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IProductViewRepository _productViewRepository;
        private readonly IUserFavoriteProductRepository _userFavoriteProductRepository;

        public ProductApplication(IProductRepository productRepository, IFileUploader fileUploader, IProductCategoryRepository productCategoryRepository, IProductViewRepository productViewRepositoryl, IUserFavoriteProductRepository userFavoriteProductRepository)
        {
            _fileUploader = fileUploader;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _productViewRepository = productViewRepositoryl;
            _userFavoriteProductRepository = userFavoriteProductRepository;
        }

        public async Task<OperationResult> Create(CreateProduct command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            if (await _productRepository.ExistsAsync(x => x.Name == command.Name && x.Code == command.Code, cancellation))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var slug = command.Slug.Slugify();
            var categoryFullSlug = await _productCategoryRepository.GetFullSlugById(command.CategoryId, cancellation);
            var path = $"ProductCategories/{categoryFullSlug}/Products/{slug}";
            var picturePath = await _fileUploader.UploadAsync(command.Picture, path, cancellation);

            var product = new Product(command.Name, command.Code, command.Brand,
                command.ShortDescription, command.Description, picturePath,
                command.PictureAlt, command.PictureTitle, command.CategoryId, slug,
                command.Keywords, command.MetaDescription, command.Specifications);

            _productRepository.Create(product);
            await _productRepository.SaveChangesAsync(cancellation);

            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditProduct command, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            var product = await _productRepository.GetProductWithCategory(command.Id, cancellation);

            if (product == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _productRepository.ExistsAsync(x => x.Name == command.Name && x.Code == command.Code && x.Id != command.Id, cancellation))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var slug = command.Slug.Slugify();
            var path = $"ProductCategories/{product.Category.FullSlug}/Products/{slug}";
            var picturePath = "";
            if (!(command.Picture == null && string.IsNullOrEmpty(command.Picture?.FileName)))
            {
                _fileUploader.DeleteFile(product.Picture);
                picturePath = await _fileUploader.UploadAsync(command.Picture, path, cancellation);
            }

            product.Edit(command.Name, command.Code, command.Brand,
                command.ShortDescription, command.Description, picturePath,
                command.PictureAlt, command.PictureTitle, command.CategoryId, slug,
                command.Keywords, command.MetaDescription, command.Specifications);

            await _productRepository.SaveChangesAsync(cancellation);

            return operation.Succeeded();
        }

        public async Task<EditProduct?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _productRepository.GetDetails(id, cancellation);
        }

        public async Task<List<ProductViewModel>> GetProducts(CancellationToken cancellation)
        {
            return await _productRepository.GetProducts(cancellation);
        }

        public async Task<OperationResult> IncreaseProductViewCountAsync(long productId, long userId = 0, CancellationToken cancellation = default)
        {
            var op = new OperationResult();

            var product = await _productRepository.GetAsync(productId, cancellation);
            if (product == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            product.AddView();

            _productViewRepository.Create(new ProductView(productId, userId));

            await _productRepository.SaveChangesAsync(cancellation);
            return op.Succeeded();
        }


        public async Task<List<ProductViewModel>> Search(ProductSearchModel searchModel, CancellationToken cancellation)
        {
            return await _productRepository.Search(searchModel, cancellation);
        }

        public async Task<OperationResult> AddFavoriteAsync(long userId, long productId, CancellationToken cancellation = default)
        {
            var op = new OperationResult();

            var existing = await _userFavoriteProductRepository
                .GetByUserIdAndProductId(userId, productId);

            if (existing != null)
                return op.Failed("این محصول قبلاً به لیست علاقه‌مندی شما اضافه شده است.");

            var favorite = new UserFavoriteProduct(userId, productId);

            _userFavoriteProductRepository.Create(favorite);
            await _userFavoriteProductRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("محصول با موفقیت به لیست علاقه‌مندی شما اضافه شد.");
        }

        public async Task<OperationResult> DeleteFavoriteAsync(long userId, long productId, CancellationToken cancellation = default)
        {
            var op = new OperationResult();

            var existing = await _userFavoriteProductRepository
                .GetByUserIdAndProductId(userId, productId);

            if (existing == null)
                return op.Failed("این محصول در لیست علاقه‌مندی شما یافت نشد.");

            _userFavoriteProductRepository.Remove(existing);
            await _userFavoriteProductRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("محصول با موفقیت از لیست علاقه‌مندی شما حذف شد.");
        }

        public async Task<List<string>?> GetDistinctBrands(CancellationToken cancellationToken)
        {
            return await _productRepository.GetDistinctBrands(cancellationToken);
        }
    }
}
