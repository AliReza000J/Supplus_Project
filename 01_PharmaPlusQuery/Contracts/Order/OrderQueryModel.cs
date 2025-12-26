using ShopManagement.Application.Contracts.Order;

namespace _01_PharmaPlusQuery.Contracts.Order
{
    public class OrderQueryModel
    {
        public long Id { get; set; }
        public long AccountId { get; set; }

        public DateTime CreationDate { get; set; }
        public string? CreationDateDisplay { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string? LastModifiedDateDisplay { get; set; }
        public DateTime SortDate { get; set; }

        public int Status { get; set; }
        public string? StatusTitle { get; set; }

        public string? PaymentMethod { get; set; }
        public int PaymentMethodInt { get; set; }
        public string? PostTrackingNumber { get; set; }

        public double TotalAmount { get; set; }
        public string? TotalAmountDisplay { get; set; }
        public double DiscountAmount { get; set; }
        public string? DiscountAmountDisplay { get; set; }
        public double PayAmount { get; set; }
        public string? PayAmountDisplay { get; set; }

        public List<OrderItemQueryModel>? Items { get; set; }

    }
}
