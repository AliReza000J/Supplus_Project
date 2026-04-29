using _0_Framework.Domain;
using _0_Framework.Application;
using ShopManagement.Domain.ProductCategoryAgg;
using ShopManagement.Domain.ProductPictureAgg;

namespace ShopManagement.Domain.ProductAgg
{
    public class Product: BaseEntity
    {
        public string Name { get; private set; }
        public long CategoryId { get; private set; }
        public string Code { get; private set; }
        public string Brand { get; private set; }
        public string ShortDescription { get; private set; }
        public string Description { get; private set; }
        public string Specifications { get; private set; }
        public string Picture { get; private set; }
        public string PictureAlt { get; private set; }
        public string PictureTitle { get; private set; }
        public string Slug { get; private set; }
        public string Keywords { get; private set; }
        public string MetaDescription { get; private set; }
        public int ViewCount { get; set; } = 0;
        public ProductCategory Category { get; private set; }
        public List<ProductPicture> ProductPictures { get; private set; }

        public Product(string name, string code, string brand, string shortDescription, string description,
            string picture, string pictureAlt, string pictureTitle, long categoryId, string slug,
            string keywords, string metaDescription, string specifications)
        {
            Name = name;
            Code = code;
            Brand = brand;
            ShortDescription = shortDescription;
            Description = description;
            Picture = picture;
            PictureAlt = pictureAlt;
            PictureTitle = pictureTitle;
            CategoryId = categoryId;
            Slug = slug;
            Keywords = keywords;
            MetaDescription = metaDescription;
            Specifications = specifications;
        }

        public void Edit(string name, string code, string brand, string shortDescription, string description, string picture,
            string pictureAlt, string pictureTitle, long categoryId, string slug,
            string keywords, string metaDescription, string specifications)
        {
            Name = name;
            Code = code;
            Brand = brand;
            ShortDescription = shortDescription;
            Description = description;

            if (!string.IsNullOrWhiteSpace(picture))
                Picture = picture;

            PictureAlt = pictureAlt;
            PictureTitle = pictureTitle;
            CategoryId = categoryId;
            Slug = slug;
            Keywords = keywords;
            MetaDescription = metaDescription;
            Specifications = specifications;
        }

        public void AddView()
        {
            ViewCount++;
        }
    }
}
