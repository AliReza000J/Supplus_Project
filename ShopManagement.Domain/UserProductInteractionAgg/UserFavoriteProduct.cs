using _0_Framework.Domain;

namespace ShopManagement.Domain.UserProductInteractionAgg
{
    public class UserFavoriteProduct: EntityBase
    {
        public long ProductId { get; set; }
        public long UserId { get; set; }

        protected UserFavoriteProduct() { }
        public UserFavoriteProduct(long userId, long productId)
        {
            UserId = userId;
            ProductId = productId;
        }
    }
}