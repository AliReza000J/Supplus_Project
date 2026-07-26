using System.Text.Json.Serialization;

namespace _01_PharmaPlusQuery.Contracts.Order
{
    public class OrderItemQueryModel
    {
        public long Id { get; set; }
        public long OrderId { get; set; }

        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductPicture { get; set; }
        public string? ProductFullPath { get; set; }

        public int Qty { get; set; }

        [JsonIgnore]
        public double UnitPrice { get; set; }
        public string? UnitPriceDisplay { get; set; }

        [JsonIgnore]
        public int DiscountRate { get; set; }
        public string? DiscountRateDisplay { get; set; }

        [JsonIgnore]
        public double TotalPriceWithDiscount { get; set; }
        public string? TotalPriceWithDiscountDisplay { get; set; }
    }
}
