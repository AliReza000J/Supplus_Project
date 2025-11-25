namespace ShopManagement.Application.Contracts.Cart
{
    public class CartItemViewModel
    {
        public string? CartKey { get; set; }
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public string? UnitPrice { get; set; }
        public string? PriceWithDiscount { get; set; }
        public string? Picture { get; set; }
    }
}
