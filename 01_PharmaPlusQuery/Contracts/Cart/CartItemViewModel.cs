namespace _01_PharmaPlusQuery.Contracts.Cart
{
    public class CartItemViewModel
    {
        public string? CartKey { get; set; }
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public string? UnitPrice { get; set; }
        public string? PriceWithDiscount { get; set; }
        public int DiscountedQty { get; set; } = 0;
        public string? Picture { get; set; }
    }
}
