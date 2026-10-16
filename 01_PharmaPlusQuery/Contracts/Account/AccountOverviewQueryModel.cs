using _01_PharmaPlusQuery.Contracts.Order;
using System.Text.Json.Serialization;

namespace _01_PharmaPlusQuery.Contracts.Account
{
    public class AccountOverviewQueryModel
    {
        public List<LatestOrderQueryModel>? LatestOrders { get; set; }
        public int TotalOrders { get; set; }
        public int TotalRefundedOrders { get; set; }
        public int TotalDeliveredOrders { get; set; }
        public int TotalFaves { get; set; }
        public int TotalAddresses { get; set; }
    }

    public class LatestOrderQueryModel
    {
        public long Id { get; set; }

        //[JsonIgnore]
        //public DateTime CreationDate { get; set; }
        public string? CreationDate { get; set; }

        public int Status { get; set; }
        public string? StatusTitle { get; set; }

        public double PayAmount { get; set; }

        public int ItemsCount { get; set; }
    }
}
