namespace _01_PharmaPlusQuery.Contracts.Order
{
    public class OrderSummaryQueryModel
    {
        public long Id { get; set; }

        public double PayAmount { get; set; }
        public string? PayAmountDisplay { get; set; }

        public double TotalAmount { get; set; }
        public string? TotalAmountDisplay { get; set; }

        public int Status { get; set; }
        public string? StatusTitle { get; set; }

        public DateTime CreationDate { get; set; }
        public string? CreationDateDisplay { get; set; }

        public string? PostTrackingNumber { get; set; }

    }
}
