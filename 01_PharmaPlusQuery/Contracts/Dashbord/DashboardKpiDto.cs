namespace _01_PharmaPlusQuery.Contracts.Dashbord
{
    public class DashboardKpiDto
    {
        public long TotalOrders { get; set; }
        public double TotalRevenue { get; set; }
        public long PaidOrders { get; set; }
        public long ActiveUsers { get; set; }
        public long TotalProductViews { get; set; }
    }
}
