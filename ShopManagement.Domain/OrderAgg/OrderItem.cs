using _0_Framework.Domain;

namespace ShopManagement.Domain.OrderAgg
{
    public class OrderItem : EntityBase
    {
        public long ProductId { get; private set; }
        public string ProductName { get; private set; } = "";
        public string Picture { get; private set; } = "";
        public int Quantity { get; private set; }
        public double UnitPrice { get; private set; }
        public long DiscountId { get; private set; }
        public int DiscountRate { get; private set; }
        public int DiscountedQty { get; private set; }
        public double TotalDiscountAmount => Math.Min(DiscountedQty, Quantity) * (UnitPrice * DiscountRate / 100);
        public double TotalPriceWithDiscount => (UnitPrice * Quantity) - TotalDiscountAmount;

        public long OrderId { get; private set; }
        public Order? Order { get; private set; }

        protected OrderItem() { }
        public OrderItem(long productId, int qty, double unitPrice, int discountRate,
            string productName, int discountedQty, long discountId, string picture = "")
        {
            ProductId = productId;
            Quantity = qty;
            UnitPrice = unitPrice;
            DiscountRate = discountRate;
            ProductName = productName;
            DiscountedQty = discountedQty;
            Picture = picture ?? string.Empty;
            DiscountId = discountId;
        }

        public OrderItem(long productId, int qty, double unitPrice)
        {
            ProductId = productId;
            Quantity = qty;
            UnitPrice = unitPrice;
        }

        public double TotalPayAmount() => TotalPriceWithDiscount;
        public double TotalDiscAmount() => TotalDiscountAmount;
        public double Total() => UnitPrice * Quantity;
    }
}