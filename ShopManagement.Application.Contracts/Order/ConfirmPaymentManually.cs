namespace ShopManagement.Application.Contracts.Order
{
    public class ConfirmPaymentManually
    {
        public long OrderId { get; set; }
        public string BankTrackingCode { get; set; }
    }

}
