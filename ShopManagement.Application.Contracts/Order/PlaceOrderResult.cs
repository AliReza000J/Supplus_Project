namespace ShopManagement.Application.Contracts.Order
{
    public class PlaceOrderResult
    {
        public long OrderId { get; set; }
        public bool ShouldRedirect { get; set; }
        public string? RedirectUrl { get; set; }
    }
}
