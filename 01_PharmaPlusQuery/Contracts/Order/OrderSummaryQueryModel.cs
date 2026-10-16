using System.Text.Json.Serialization;

namespace _01_PharmaPlusQuery.Contracts.Order
{
    public class OrderSummaryQueryModel
    {
        public long Id { get; set; }

        public double PayAmount { get; set; }

        public double TotalAmount { get; set; }

        public int Status { get; set; }
        public string? StatusTitle { get; set; }

        //[JsonIgnore]
        //public DateTime CreationDate { get; set; }
        public string? CreationDate { get; set; }

        public string? PostTrackingNumber { get; set; }

        public int ItemsCount { get; set; }
    }
}
