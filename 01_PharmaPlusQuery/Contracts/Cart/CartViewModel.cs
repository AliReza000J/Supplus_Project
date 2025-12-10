namespace _01_PharmaPlusQuery.Contracts.Cart
{
    public class CartViewModel
    {
        public string? CartKey { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new();
        public string? TotalAmount { get; set; }
    }
}