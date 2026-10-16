namespace _01_PharmaPlusQuery.Contracts.Product
{
    public enum ProductSortOption
    {
        Newest = 1,
        PriceAsc = 2,
        PriceDesc = 3,
        Popularity = 4,
        HighestDiscount = 5,
        MostViewed = 6,
        TopSales = 7,
    }
    public class ProductFilterRequest
    {
        public string? SearchTerm { get; set; }
        public string? CategoryFullSlug { get; set; }
        public string? Brand { get; set; }

        public string? CountryOfOrigin { get; set; }
        public string? ProductForm { get; set; }

        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public bool OnlyInStock { get; set; }
        public bool HasDiscount { get; set; }

        public ProductSortOption SortBy { get; set; } = ProductSortOption.Newest;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}