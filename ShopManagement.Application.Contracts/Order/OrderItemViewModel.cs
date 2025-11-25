namespace ShopManagement.Application.Contracts.Order
{
    public class OrderItemViewModel
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string Picture { get; set; }

        public int Count { get; set; }
        public double UnitPrice { get; set; }
        public double DiscountRate { get; set; }
        public double TotalItemPrice { get; set; }
    }
}