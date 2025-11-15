using _0_Framework.Domain;

namespace ShopManagement.Domain.OrderAgg
{
    public class OrderItem : BaseEntity
    {
        public long ProductId { get; private set; }
        public string ProductName { get; private set; } = "";
        public string Picture { get; private set; } = "";
        public int Quantity { get; private set; }
        public double UnitPrice { get; private set; }
        public int DiscountRate { get; private set; }
        public int DiscountApplied { get; private set; }
        public double TotalDiscountAmount => DiscountApplied * (UnitPrice * DiscountRate);
        public double TotalPriceWithDiscount => (UnitPrice * Quantity) - TotalDiscountAmount;

        public long OrderId { get; private set; }
        public Order Order { get; private set; }
        //public long? InventoryId { get; private set; }

        protected OrderItem() { }
        public OrderItem(long productId, int qty, double unitPrice, int discountRate,
            string productName, int discountApplied, string picture = "")
        {
            ProductId = productId;
            Quantity = qty;
            UnitPrice = unitPrice;
            DiscountRate = discountRate;
            ProductName = productName;
            //InventoryId = inventoryId;
            DiscountApplied = discountApplied;
            Picture = picture ?? string.Empty;
        }
        public double Total() => TotalPriceWithDiscount;
    }
}