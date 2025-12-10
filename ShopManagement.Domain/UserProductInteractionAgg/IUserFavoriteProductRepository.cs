using _0_Framework.Domain;

namespace ShopManagement.Domain.UserProductInteractionAgg
{
    public interface IUserFavoriteProductRepository: IRepository<long, UserFavoriteProduct>
    {
        Task<UserFavoriteProduct?> GetByUserIdAndProductId(string userId, long productId);
        void Remove(UserFavoriteProduct ufp);
    }
}
