using _0_Framework.Application;
using ShopManagement.Application.Contracts.ProductPicture;
using ShopManagement.Domain.ProductAgg;
using ShopManagement.Domain.ProductPictureAgg;

namespace ShopManagement.Application
{
    public class ProductPictureApplication : IProductPictureApplication
    {
        private readonly IFileUploader _fileUploader;
        private readonly IProductRepository _productRepository;
        private readonly IProductPictureRepository _productPictureRepository;

        public ProductPictureApplication(IProductPictureRepository productPictureRepository, IProductRepository productRepository, IFileUploader fileUploader)
        {
            _fileUploader = fileUploader;
            _productRepository = productRepository;
            _productPictureRepository = productPictureRepository;
        }

        public async Task<OperationResult> Create(CreateProductPicture command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            
            var product = await _productRepository.GetProductWithCategory(command.ProductId, cancellation);

            var path = $"ProductCategories/{product?.Category.FullSlug}/Products/{product?.Slug}";
            var picturePath = await _fileUploader.UploadAsync(command.Picture, path, cancellation);

            var productPicture = new ProductPicture(command.ProductId, picturePath, command.PictureAlt, command.PictureTitle);
            _productPictureRepository.Create(productPicture);
            await _productPictureRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditProductPicture command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var productPicture = await _productPictureRepository.GetWithProductAndCategory(command.Id, cancellation);
            if (productPicture == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            var path = $"ProductCategories/{productPicture.Product.Category.FullSlug}/Products/{productPicture.Product.Slug}";
            var picturePath = "";

            if (!(command.Picture == null && string.IsNullOrEmpty(command.Picture?.FileName)))
            {
                _fileUploader.DeleteFile(productPicture.Picture);
                picturePath = await _fileUploader.UploadAsync(command.Picture, path, cancellation);
            }

            productPicture.Edit(command.ProductId, picturePath, command.PictureAlt, command.PictureTitle);
            await _productPictureRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<EditProductPicture?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _productPictureRepository.GetDetails(id, cancellation);
        }

        public async Task<OperationResult> Remove(long id, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var productPicture = await _productPictureRepository.GetAsync(id, cancellation);
            if (productPicture == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            productPicture.Remove();
            await _productPictureRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<OperationResult> Restore(long id, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var productPicture = await _productPictureRepository.GetAsync(id, cancellation);
            if (productPicture == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            productPicture.Restore();
            await _productPictureRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<List<ProductPictureViewModel>> Search(ProductPictureSearchModel searchModel, CancellationToken cancellation)
        {
            return await _productPictureRepository.Search(searchModel, cancellation);
        }
    }
}
