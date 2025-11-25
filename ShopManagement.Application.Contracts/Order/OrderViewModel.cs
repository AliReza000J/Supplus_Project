namespace ShopManagement.Application.Contracts.Order
{
    public class OrderViewModel
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public string CustomerName { get; set; }
        public string Mobile { get; set; }

        public string CreationDate { get; set; }

        public int Status { get; set; }
        public string StatusTitle { get; set; }

        public string PaymentMethod { get; set; }   // Online / COD / BankTransfer
        public bool IsPaid { get; set; }
        public string TrackingNumber { get; set; }

        public double TotalAmount { get; set; }
        public double DiscountAmount { get; set; }
        public double PayAmount { get; set; }

        public List<OrderItemViewModel> Items { get; set; }
    }

}