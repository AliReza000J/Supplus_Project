namespace ShopManagement.Application.Contracts.Order
{
    public class SetTrackingNumber
    {
        public long OrderId { get; set; }
        public string TrackingNumber { get; set; }
    }

}
