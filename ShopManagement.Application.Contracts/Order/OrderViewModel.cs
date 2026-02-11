namespace ShopManagement.Application.Contracts.Order
{
    public class OrderViewModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? ReceiverFullName { get; set; }
        public string? ReceiverMobile { get; set; }
        public string? ReceiverAddress { get; set; }
        public string? ReceiverZipCode { get; set; }

        public string? CreationDate { get; set; }
        public string? LastModifiedDate { get; set; }

        public int Status { get; set; }
        public string? StatusTitle { get; set; }

        public string? PaymentMethod { get; set; }   
        public int PaymentMethodInt { get; set; }   
        public string? PostTrackingNumber { get; set; }
        public string? RefNumber { get; set; }

        public double TotalAmount { get; set; }
        public double DiscountAmount { get; set; }
        public double PayAmount { get; set; }

        public string? CardOwnerNationalCode { get; set; }
        public string? CardOwnerFullname { get; set; }
        public string? PaymentReceiptPic { get; set; }

        public string? CanellationReason { get; set; }

        public List<OrderItemViewModel> Items { get; set; }
    }

}