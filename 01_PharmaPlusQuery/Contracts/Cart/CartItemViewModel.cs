namespace _01_PharmaPlusQuery.Contracts.Cart
{
    public class CartItemViewModel
    {
        public string? CartKey { get; set; }
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Qty { get; set; }
        public int InvQty { get; set; }
        public string? UnitPrice { get; set; }
        public string? PriceWithDiscount { get; set; }
        public int DiscountedQty { get; set; } = 0;
        public int DiscountRate { get; set; } = 0;
        public bool HasDiscount { get; set; } = false;
        public string? Picture { get; set; }
    }
}
