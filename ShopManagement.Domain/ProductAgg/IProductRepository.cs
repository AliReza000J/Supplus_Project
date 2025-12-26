using _0_Framework.Domain;
using ShopManagement.Application.Contracts.Product;

namespace ShopManagement.Domain.ProductAgg
{
    public interface IProductRepository: IRepository<long, Product>
    {
        Task<string?> GetProductName(long prodcutId);
        Task<string?> GetProductPic(long prodcutId);
        Task<EditProduct?> GetDetails(long id);
        Task<Product?> GetProductWithCategory(long id);
        Task<List<ProductViewModel>> GetProducts();
        Task<List<ProductViewModel>> Search(ProductSearchModel searchModel);
    }
}