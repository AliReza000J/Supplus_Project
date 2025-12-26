namespace ShopManagement.Application.Contracts.Order
{
    public class VerifyGatewayPaymentCommand
    {
        public long OrderId { get; set; }

        public string TrackId { get; set; } = "";

    }

}
