using AccountManagement.Domain.ShippingInfoAgg;
using System.Text.Json.Serialization;

namespace _01_PharmaPlusQuery.Contracts.Order
{
    public class OrderQueryModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }

        [JsonIgnore]
        public DateTime CreationDate { get; set; }
        public string? CreationDateDisplay { get; set; }

        [JsonIgnore]
        public DateTime LastModifiedDate { get; set; }
        public string? LastModifiedDateDisplay { get; set; }

        public int Status { get; set; }
        public string? StatusTitle { get; set; }

        public string? PaymentMethod { get; set; }

        [JsonIgnore]
        public int PaymentMethodInt { get; set; }

        public string? PostTrackingNumber { get; set; }

        [JsonIgnore]
        public double TotalAmount { get; set; }
        public string? TotalAmountDisplay { get; set; }

        [JsonIgnore]
        public double DiscountAmount { get; set; }
        public string? DiscountAmountDisplay { get; set; }

        public string? CouponCode { get; set; }
        public long? AppliedCouponId { get; set; }

        [JsonIgnore]
        public double OrderCouponAmount { get; set; }
        public string? OrderCouponAmountDisplay { get; set; }

        [JsonIgnore]
        public double PayAmount { get; set; }
        public string? PayAmountDisplay { get; set; }

        public List<OrderItemQueryModel>? Items { get; set; }

        public string? ReceiverFullName { get; set; }
        public string? ReceiverMobile { get; set; }
        public string? ReceiverAddress { get; set; }
        public string? ReceiverZipCode { get; set; }
    }
}
