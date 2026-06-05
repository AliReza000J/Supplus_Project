namespace _01_PharmaPlusQuery.Contracts.Dashbord
{
    public class TopCustomerDto
    {
        public long AccountId { get; set; }
        public string Username { get; set; }
        public double TotalSpent { get; set; }
        public int OrderCount { get; set; }
    }

}
