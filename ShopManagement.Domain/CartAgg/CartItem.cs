using _0_Framework.Domain;

namespace ShopManagement.Domain.CartAgg
{
    public class CartItem : BaseEntity
    {
        public string CartKey { get; private set; }
        public long ProductId { get; private set; }
        public int Quantity { get; private set; }

        protected CartItem() { } // EF Core

        public CartItem(long productId, int qty, string cartKey)
        {
            ProductId = productId;
            Quantity = qty;
            CartKey = cartKey;
        }

        public void Increase(int qty) => Quantity += qty;
        public void ChangeQuantity(int qty) => Quantity = qty;
    }

}
