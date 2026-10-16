using System.Text.Json.Serialization;

namespace _01_PharmaPlusQuery.Contracts.Cart
{
    public class CartViewModel
    {
        [JsonIgnore]
        public string? CartKey { get; set; }

        public double TotalAmount { get; set; } = 0;
        public double TotalDiscountAmount { get; set; } = 0;
        public double TotalFinalAmount { get; set; } = 0;

        public List<CartItemViewModel> Items { get; set; } = new();
    }
}