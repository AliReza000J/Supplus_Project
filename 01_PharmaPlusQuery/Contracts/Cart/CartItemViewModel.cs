using System.Text.Json.Serialization;

namespace _01_PharmaPlusQuery.Contracts.Cart
{
    public class CartItemViewModel
    {
        [JsonIgnore]
        public string? CartKey { get; set; }

        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductFullSlug { get; set; }
        public string? Picture { get; set; }

        public int Qty { get; set; } = 0;
        public int InvQty { get; set; } = 0;

        public int DiscountedQty { get; set; } = 0;
        public int DiscountRate { get; set; } = 0;
        public bool HasDiscount { get; set; } = false;

        public double UnitPrice { get; set; } = 0;
        public double PriceWithDiscount { get; set; } = 0;
        public double TotalItemPrice { get; set; } = 0;
    }
}
