using _0_Framework.Domain;

namespace DiscountManagement.Domain.DiscountAgg
{
    public class DiscountTarget : BaseEntity
    {
        public DiscountTargetType TargetType { get; private set; }

        public long? ProductId { get; private set; }
        public long? CategoryId { get; private set; }
        public string? BrandName { get; private set; }

        private DiscountTarget() { }

        private DiscountTarget(DiscountTargetType type)
        {
            TargetType = type;
        }

        public static DiscountTarget ForProduct(long productId)
            => new(DiscountTargetType.Product) { ProductId = productId };

        public static DiscountTarget ForCategory(long categoryId)
            => new(DiscountTargetType.Category) { CategoryId = categoryId };

        public static DiscountTarget ForBrand(string brandName)
            => new(DiscountTargetType.Brand) { BrandName = brandName };

        public static DiscountTarget ForAll()
            => new(DiscountTargetType.All);

        public enum DiscountTargetType
        {
            Product = 1,
            Brand = 2,
            Category = 3,
            All = 4
        }
    }

}
