namespace _01_PharmaPlusQuery.Contracts.Dashbord
{
    public class RecentOrderDto
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public string CustomerUsername { get; set; }
        public double PayAmount { get; set; }
        public string StatusChangedAt { get; set; }
        public int Status { get; set; }
    }

}
