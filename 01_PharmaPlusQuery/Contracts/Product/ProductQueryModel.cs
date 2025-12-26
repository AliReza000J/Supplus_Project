using _01_LampshadeQuery.Contracts.Comment;

namespace _01_PharmaPlusQuery.Contracts.Product
{
    public class ProductQueryModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public string CategoryName { get; set; }
        public string CategoryFullSlug { get; set; }
        public string CategorySlug { get; set; }

        public string Code { get; set; }
        public string Brand { get; set; }
        public string ShortDescription { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Specifications { get; set; }
        public string Keywords { get; set; }
        public string MetaDescription { get; set; }

        public string Picture { get; set; }
        public string PictureAlt { get; set; }
        public string PictureTitle { get; set; }

        public double DoublePrice { get; set; }
        public string Price { get; set; }

        public string PriceWithDiscount { get; set; }
        public int DiscountRate { get; set; }
        public bool HasDiscount { get; set; }
        public int DiscountedQty { get; set; } = 0;
        public string DiscountExpireDate { get; set; }

        public bool IsInStock { get; set; }
        public long InStockQty { get; set; } = 0;

        public int CommentCount { get; set; }
        public float AvgRate { get; set; }
        public int RateCount { get; set; }

        public int ViewCount { get; set; }
        public string? RecentViewsLabel { get; set; } = null;

        public DateTime? UserFavedDate { get; set; } = null;
        public string? UserFavedDateStr { get; set; } = null;
        public bool CurrentUserFaved { get; set; }

        public List<CommentQueryModel> Comments { get; set; }
        public List<ProductPictureQueryModel> Pictures { get; set; }
    }
}