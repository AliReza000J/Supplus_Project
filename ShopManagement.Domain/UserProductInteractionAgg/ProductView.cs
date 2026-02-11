using _0_Framework.Domain;

namespace ShopManagement.Domain.UserProductInteractionAgg
{
    public class ProductView : BaseEntity
    {
        public long ProductId { get; set; }
        public long UserId { get; set; }

        protected ProductView()
        {

        }
        public ProductView(long productId, long userId)
        {
            ProductId = productId;
            UserId = userId;
        }
    }
}
