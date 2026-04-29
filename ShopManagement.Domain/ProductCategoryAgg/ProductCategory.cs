using _0_Framework.Domain;
using _0_Framework.Application;
using ShopManagement.Domain.ProductAgg;

namespace ShopManagement.Domain.ProductCategoryAgg
{
    public class ProductCategory: BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Picture { get; private set; }
        public string PictureAlt { get; private set; }
        public string PictureTitle { get; private set; }
        public string Keywords { get; private set; }
        public string MetaDescription { get; private set; }
        public string Slug { get; private set; }
        public string FullSlug { get; private set; }
        public List<Product> Products { get; private set; }

        // Self referencing
        public long? ParentId { get; private set; }
        public ProductCategory? Parent { get; private set; }
        public List<ProductCategory> Children { get; private set; }

        public ProductCategory()
        {
            Products = new List<Product>();
            Children = new List<ProductCategory>();
        }

        public ProductCategory(string name, string description, string picture, string pictureAlt,
            string pictureTitle, string slug, string fullSlug, string metaDescription, string keywords, long? parentId)
        {
            Name = name;
            Description = description;
            Picture = picture;
            PictureAlt = pictureAlt;
            PictureTitle = pictureTitle;
            Slug = slug;
            FullSlug = fullSlug;
            MetaDescription = metaDescription;
            Keywords = keywords;
            ParentId = parentId;
        }

        public void Edit(string name, string description, string picture, string pictureAlt,
            string pictureTitle, string slug, string fullslug, string metaDescription, string keywords, long? parentId)
        {
            Name = name;
            Description = description;

            if (!string.IsNullOrWhiteSpace(picture))
                Picture = picture;

            PictureAlt = pictureAlt;
            PictureTitle = pictureTitle;
            Slug = slug;
            FullSlug = fullslug;
            MetaDescription = metaDescription;
            Keywords = keywords;
            ParentId = parentId;
        }

        public void AddChild(ProductCategory child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (IsDescendantOf(child.Id)) throw new InvalidOperationException("Cannot add ancestor as child (would create cycle).");
            child.ParentId = this.Id;
            child.Parent = this;
            Children.Add(child);
        }

        // helper to avoid cycles (requires children loaded or use repository checks)
        public bool IsDescendantOf(long candidateAncestorId)
        {
            var p = Parent;
            while (p != null)
            {
                if (p.Id == candidateAncestorId) return true;
                p = p.Parent;
            }
            return false;
        }

        public void SetFullSlug(string fullSlug)
        {
            FullSlug = fullSlug;
        }
    }
}
