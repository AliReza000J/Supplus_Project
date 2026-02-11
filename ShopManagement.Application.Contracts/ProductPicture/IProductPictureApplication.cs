using _0_Framework.Application;

namespace ShopManagement.Application.Contracts.ProductPicture
{
    public interface IProductPictureApplication
    {
        Task<OperationResult> Create(CreateProductPicture command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditProductPicture command, CancellationToken cancellation);
        Task<OperationResult> Remove(long id, CancellationToken cancellation);
        Task<OperationResult> Restore(long id, CancellationToken cancellation);
        Task<EditProductPicture?> GetDetails(long id, CancellationToken cancellation);
        Task<List<ProductPictureViewModel>> Search(ProductPictureSearchModel searchModel, CancellationToken cancellation);
    }
}
