namespace ShopManagement.Application.Contracts.Order
{
    /// <summary>
    /// Represents the command to verify a payment made through an online gateway.
    /// </summary>
    public class VerifyGatewayPaymentCommand
    {
        /// <summary>
        /// The unique identifier of the order associated with the payment.
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// The tracking ID (Token or Authority) returned by the payment gateway during the callback.
        /// </summary>
        public string TrackId { get; set; } = "";
    }
}
