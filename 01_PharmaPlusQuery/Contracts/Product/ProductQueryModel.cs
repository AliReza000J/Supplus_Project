using _01_PharmaPlusQuery.Contracts.Comment;

namespace _01_PharmaPlusQuery.Contracts.Product
{
    public class ProductQueryModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryFullSlug { get; set; }
        public string CategorySlug { get; set; }

        public string Code { get; set; }
        public string Brand { get; set; }
        public string ShortDescription { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public ProductSpecificationModel? Specifications { get; set; }
        public string Keywords { get; set; }
        public string MetaDescription { get; set; }

        public string Picture { get; set; }
        public string PictureAlt { get; set; }
        public string PictureTitle { get; set; }

        public double Price { get; set; }

        public double PriceWithDiscount { get; set; }
        public int DiscountRate { get; set; }
        public bool HasDiscount { get; set; }
        public long DiscountedQty { get; set; } = 0;
        public string? DiscountExpireDate { get; set; }

        public bool IsInStock { get; set; }
        public long InvQty { get; set; } = 0;

        public int CommentCount { get; set; }
        public float AvgRate { get; set; }
        public int RateCount { get; set; }
        public int InFavedCount { get; set; }
        public int InCartCount { get; set; }
        public int ViewCount { get; set; }

        public DateTime? CurrentUserFavedDate { get; set; } = null;
        public string? CurrentUserFavedDateStr { get; set; } = null;
        public bool IsCurrentUserFaved { get; set; }

        public List<CommentQueryModel> Comments { get; set; }
        public List<ProductPictureQueryModel> Pictures { get; set; }
    }

    public class ProductSpecificationModel
    {
        public string CountryOfOrigin { get; set; } = string.Empty;
        public string ProductForm { get; set; } = string.Empty;
        public string ProductWeight { get; set; } = string.Empty;
        public Dictionary<string, string> Attributes { get; set; } = new();
    }
}