using _0_Framework.Domain;

namespace ShopManagement.Domain.UserProductInteractionAgg
{
    public interface IUserFavoriteProductRepository: IRepository<long, UserFavoriteProduct>
    {
        Task<UserFavoriteProduct?> GetByUserIdAndProductId(long userId, long productId);
        void Remove(UserFavoriteProduct ufp);
    }
}
