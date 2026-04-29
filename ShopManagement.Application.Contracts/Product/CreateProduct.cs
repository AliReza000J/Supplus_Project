using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using ShopManagement.Application.Contracts.ProductCategory;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Product
{
    public class CreateProduct
    {
        private string _name;
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(255, ErrorMessage = ValidationMessages.MaxLength)]
        public string Name
        {
            get => _name;
            set => _name = value.ToEnglishNumber();
        }

        private string _code;
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(65, ErrorMessage = ValidationMessages.MaxLength)]
        public string Code
        {
            get => _code;
            set => _code = value.ToEnglishNumber();
        }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(80, ErrorMessage = ValidationMessages.MaxLength)]
        public string Brand { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(500, ErrorMessage = ValidationMessages.MaxLength)]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Description { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Specifications { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(2 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public IFormFile Picture { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(255, ErrorMessage = ValidationMessages.MaxLength)]
        public string PictureAlt { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(500, ErrorMessage = ValidationMessages.MaxLength)]
        public string PictureTitle { get; set; }

        [Range(1, 100000, ErrorMessage = ValidationMessages.IsRequired)]
        public long CategoryId { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(500, ErrorMessage = ValidationMessages.MaxLength)]
        public string Slug { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
        public string Keywords { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(150, ErrorMessage = ValidationMessages.MaxLength)]
        public string MetaDescription { get; set; }
        public List<ProductCategoryViewModel> Categories { get; set; } = new();
    }
}
