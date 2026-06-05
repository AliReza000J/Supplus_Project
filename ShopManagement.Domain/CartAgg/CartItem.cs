using _0_Framework.Domain;

namespace ShopManagement.Domain.CartAgg
{
    public class CartItem : EntityBase
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

        public void IncreaseByQty(int qty) => Quantity += qty;
        public void Increase() => Quantity ++;
        public void Decrease() => Quantity--;
        public void ChangeQuantity(int qty) => Quantity = qty;
    }

}
