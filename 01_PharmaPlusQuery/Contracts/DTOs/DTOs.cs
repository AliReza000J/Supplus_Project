using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Product;

namespace _01_PharmaPlusQuery.Contracts.DTOs
{
    public class ProductInventoryDto
    {
        public long ProductId { get; set; }
        public double UnitPrice { get; set; }
        public bool InStock { get; set; }
    }

    public class ProductDiscountDto
    {
        public long ProductId { get; set; }
        public int DiscountRate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SearchByBrandResult
    {
        public PagedResult<ProductQueryModel> Products { get; set; }
        public List<string> Categories { get; set; }
    }

}
