namespace ShopManagement.Application.Contracts.Cart
{
    public class IncOrDecItem
    {
        public string? CartKey { get; set; }
        public long ProductId { get; set; }
    }
}
