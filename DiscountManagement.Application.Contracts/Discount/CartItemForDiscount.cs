namespace DiscountManagement.Application.Contracts.Discount
{
    public class CartItemForDiscount
    {
        public long ProductId { get; set; }
        public long CategoryId { get; set; }
        public string Brand { get; set; } = "";
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
