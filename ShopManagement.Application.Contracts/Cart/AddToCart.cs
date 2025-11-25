namespace ShopManagement.Application.Contracts.Cart
{
    public class AddToCart
    {
        public string? CartKey { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
