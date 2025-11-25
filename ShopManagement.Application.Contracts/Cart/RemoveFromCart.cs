namespace ShopManagement.Application.Contracts.Cart
{
    public class RemoveFromCart
    {
        public string? CartKey { get; set; }
        public long ProductId { get; set; }
    }
}
