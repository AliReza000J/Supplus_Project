using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BlogManagement.Application.Contracts.Article
{
    public class CreateArticle
    {
        [MaxLength(500, ErrorMessage = ValidationMessages.MaxLength)]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = ValidationMessages.MaxLength)]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Body { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [FileExtensionLimitation(new string[] { ".png", ".jpg", ".jpeg"}, ErrorMessage =ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(2 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public IFormFile Picture { get; set; }

        [MaxLength(500, ErrorMessage = ValidationMessages.MaxLength)]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string PictureAlt { get; set; }

        [MaxLength(500, ErrorMessage = ValidationMessages.MaxLength)]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string PictureTitle { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string PublishDate { get; set; }

        [MaxLength(500, ErrorMessage = ValidationMessages.MaxLength)]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Slug { get; set; }

        [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Keywords { get; set; }

        [MaxLength(150, ErrorMessage = ValidationMessages.MaxLength)]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string MetaDescription { get; set; }

        [MaxLength(1000, ErrorMessage = ValidationMessages.MaxLength)]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string CanonicalAddress { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = ValidationMessages.IsRequired)]
        public long CategoryId { get; set; }
    }
}
