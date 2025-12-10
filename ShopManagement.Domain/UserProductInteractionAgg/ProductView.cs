using _0_Framework.Domain;

namespace ShopManagement.Domain.UserProductInteractionAgg
{
    public class ProductView: BaseEntity
    {
        public long ProductId { get; set; }
        public string? UserId { get; set; }

        protected ProductView()
        {
            
        }
        public ProductView(long productId, string? userId)
        {
            ProductId = productId;
            if(string.IsNullOrWhiteSpace(userId))
                UserId = "-";
            else
                UserId = userId;
        }
    }
}
