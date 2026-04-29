using _0_Framework.Application;

namespace ShopManagement.Application.Contracts.ProductCategory
{
    public class ProductCategorySearchModel
    {
        private string? _name;
        public string? Name
        {
            get => _name;
            set => _name = value?.ToEnglishNumber();
        }
    }
}
