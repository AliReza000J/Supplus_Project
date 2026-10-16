namespace _01_PharmaPlusQuery.Contracts.Order
{
    public class OrderItemQueryModel
    {
        //public long Id { get; set; }
        public long OrderId { get; set; }

        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductPicture { get; set; }
        public string? ProductFullSlug { get; set; }

        public int Qty { get; set; }

        public double UnitPrice { get; set; }

        public int DiscountRate { get; set; }

        public double TotalPriceWithDiscount { get; set; }
    }
}
