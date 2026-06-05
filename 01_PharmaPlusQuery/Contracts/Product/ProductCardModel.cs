namespace _01_PharmaPlusQuery.Contracts.Product
{
    public class ProductCardModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryFullSlug { get; set; }

        public string Brand { get; set; }
        public string Slug { get; set; }

        public ProductSpecificationModel? Specifications { get; set; }

        public string Picture { get; set; }
        public string PictureAlt { get; set; }
        public string PictureTitle { get; set; }

        public double DoublePrice { get; set; }
        public string Price { get; set; }

        public double DoublePriceWithDiscount { get; set; }
        public string PriceWithDiscount { get; set; }

        public int DiscountRate { get; set; }
        public bool HasDiscount { get; set; }
        public string? DiscountExpireDate { get; set; }

        public bool IsInStock { get; set; }

        public float AvgRate { get; set; }
        public int RateCount { get; set; }

        public int ViewCount { get; set; }
    }
    public class FaveProductsCardModel : ProductCardModel
    {
        public DateTime? CurrentUserFavedDate { get; set; } = null;
        public string? CurrentUserFavedDateStr { get; set; } = null;
    }
}