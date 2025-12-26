namespace ShopManagement.Application.Contracts.Order
{
    public class OrderItemViewModel
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductPicture { get; set; }

        public int Qty { get; set; }
        public double UnitPrice { get; set; }
        public double DiscountRate { get; set; }
        public int DiscountedQty { get; set; }
        public double TotalDiscountAmount { get; set; }
        public double TotalPriceWithDiscount { get; set; }
    }
}