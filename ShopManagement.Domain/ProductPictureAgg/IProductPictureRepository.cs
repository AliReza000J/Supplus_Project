using _0_Framework.Domain;
using ShopManagement.Application.Contracts.ProductPicture;

namespace ShopManagement.Domain.ProductPictureAgg
{
    public interface IProductPictureRepository : IRepository<long, ProductPicture>
    {
        Task<EditProductPicture?> GetDetails(long id, CancellationToken cancellation);
        Task<ProductPicture?> GetWithProductAndCategory(long id, CancellationToken cancellation);
        Task<List<ProductPictureViewModel>> Search(ProductPictureSearchModel searchModel, CancellationToken cancellation);
    }
}
