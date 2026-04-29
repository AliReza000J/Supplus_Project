using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.ProductCategory
{
    public class CreateProductCategory
    {
        private string _name;
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(250, ErrorMessage = ValidationMessages.MaxLength)]
        public string Name
        {
            get => _name;
            set => _name = value.ToEnglishNumber();
        }

        public string Description { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(1 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public IFormFile Picture { get; set; }
        
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(255, ErrorMessage = ValidationMessages.MaxLength)]
        public string PictureAlt { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(500, ErrorMessage = ValidationMessages.MaxLength)]
        public string PictureTitle { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(80, ErrorMessage = ValidationMessages.MaxLength)]
        public string Keywords { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(150, ErrorMessage = ValidationMessages.MaxLength)]
        public string MetaDescription { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Slug { get; set; }
        public List<ProductCategoryViewModel>? Categories { get; set; }
        public long? ParentId { get; set; }
    }
}
