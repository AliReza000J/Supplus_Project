namespace ShopManagement.Application.Contracts.Cart
{
    public class CartViewModel
    {
        public string? CartKey { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new();
        public double TotalAmount { get; set; }
    }
}