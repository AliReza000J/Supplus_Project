namespace ShopManagement.Application.Contracts.Order
{
    public class OrderSearchModel
    {
        public long? AccountId { get; set; }
        public long? OrderId { get; set; }
        public string? Mobile { get; set; }
        public int? Status { get; set; } = null;
        public int? PaymentMethod { get; set; }

        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }
}