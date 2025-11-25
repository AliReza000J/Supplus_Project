namespace ShopManagement.Application.Contracts.Order
{
    public class OrderSearchModel
    {
        public long? OrderId { get; set; }
        public string Mobile { get; set; }
        public int? Status { get; set; }
        public string PaymentMethod { get; set; }
        public bool? IsPaid { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}