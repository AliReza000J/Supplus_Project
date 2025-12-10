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

        public async Task<OperationResult> Create(CreateProductPicture command)
        {
            var operation = new OperationResult();
            
            var product = await _productRepository.GetProductWithCategory(command.ProductId);

            var path = $"ProductCategories/{product.Category.FullSlug}/Products/{product.Slug}";
            var picturePath = _fileUploader.Upload(command.Picture, path);

            var productPicture = new ProductPicture(command.ProductId, picturePath, command.PictureAlt, command.PictureTitle);
            _productPictureRepository.Create(productPicture);
            await _productPictureRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditProductPicture command)
        {
            var operation = new OperationResult();
            var productPicture = await _productPictureRepository.GetWithProductAndCategory(command.Id);
            if (productPicture == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            var path = $"ProductCategories/{productPicture.Product.Category.FullSlug}/Products/{productPicture.Product.Slug}";
            var picturePath = _fileUploader.Upload(command.Picture, path);

            productPicture.Edit(command.ProductId, picturePath, command.PictureAlt, command.PictureTitle);
            await _productPictureRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<EditProductPicture?> GetDetails(long id)
        {
            return await _productPictureRepository.GetDetails(id);
        }

        public async Task<OperationResult> Remove(long id)
        {
            var operation = new OperationResult();
            var productPicture = await _productPictureRepository.GetAsync(id);
            if (productPicture == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            productPicture.Remove();
            await _productPictureRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<OperationResult> Restore(long id)
        {
            var operation = new OperationResult();
            var productPicture = await _productPictureRepository.GetAsync(id);
            if (productPicture == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            productPicture.Restore();
            await _productPictureRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<List<ProductPictureViewModel>> Search(ProductPictureSearchModel searchModel)
        {
            return await _productPictureRepository.Search(searchModel);
        }
    }
}
