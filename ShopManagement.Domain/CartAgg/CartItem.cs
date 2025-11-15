using _0_Framework.Domain;

namespace ShopManagement.Domain.CartAgg
{
    public class CartItem : BaseEntity
    {
        public long ProductId { get; private set; }
        public string Picture { get; private set; } = "";
        public int Quantity { get; private set; }
        public double UnitPrice { get; private set; } // snapshot price

        protected CartItem() { } // EF Core

        public CartItem(long productId, int qty, double unitPrice, string picture = "")
        {
            ProductId = productId;
            Quantity = qty;
            UnitPrice = unitPrice;
            Picture = picture ?? string.Empty;
        }

        public void Increase(int qty) => Quantity += qty;
        public void ChangeQuantity(int qty) => Quantity = qty;
    }

}
