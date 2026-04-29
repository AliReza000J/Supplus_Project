using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Product
{
    public class ProductSearchModel
    {
        private string? _name;
        public string? Name
        {
            get => _name;
            set => _name = value?.ToEnglishNumber();
        }

        private string? _code;
        public string? Code
        {
            get => _code;
            set => _code = value?.ToEnglishNumber();
        }
        public string? Brand { get; set; }
        public long CategoryId { get; set; }
    }
}
