namespace _01_PharmaPlusQuery.Contracts.DTOs
{
    public class ProductInventoryDto
    {
        public long ProductId { get; set; }
        public double UnitPrice { get; set; }
    }

    public class ProductDiscountDto
    {
        public long ProductId { get; set; }
        public int DiscountRate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
