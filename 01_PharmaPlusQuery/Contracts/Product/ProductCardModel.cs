using System.Text.Json.Serialization;

namespace _01_PharmaPlusQuery.Contracts.Product
{
    public class ProductCardModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryFullSlug { get; set; }

        public string Brand { get; set; }
        public string Slug { get; set; }

        [JsonIgnore]
        public ProductSpecificationModel? Specifications { get; set; }

        public string Picture { get; set; }
        public string PictureAlt { get; set; }
        public string PictureTitle { get; set; }

        public double Price { get; set; }

        public double PriceWithDiscount { get; set; }

        public int DiscountRate { get; set; } = 0;
        public bool HasDiscount { get; set; } = false;
        public long DiscountedQty { get; set; } = 0;
        public string? DiscountExpireDate { get; set; }

        public bool IsInStock { get; set; } = false;
        public long InvQty { get; set; } = 0;

        public float AvgRate { get; set; } = 0;
        public int RateCount { get; set; } = 0;

        public int ViewCount { get; set; }

        public int Sales { get; set; } = 0;
    }
    public class FaveProductsCardModel : ProductCardModel
    {
        //[JsonIgnore]
        //public DateTime? CurrentUserFavedDate { get; set; } = null;
        public string? CurrentUserFavedDate { get; set; } = null;
    }
}