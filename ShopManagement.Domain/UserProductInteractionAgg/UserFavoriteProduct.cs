using _0_Framework.Domain;

namespace ShopManagement.Domain.UserProductInteractionAgg
{
    public class UserFavoriteProduct: BaseEntity
    {
        public long ProductId { get; set; }
        public string UserId { get; set; }

        protected UserFavoriteProduct() { }
        public UserFavoriteProduct(string userId, long productId)
        {
            UserId = userId;
            ProductId = productId;
        }
    }
}