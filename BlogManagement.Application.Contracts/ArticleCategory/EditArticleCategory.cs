using _0_Framework.Application;
using Microsoft.AspNetCore.Http;

namespace BlogManagement.Application.Contracts.ArticleCategory
{
    public class EditArticleCategory : CreateArticleCategory
    {
        public long Id { get; set; }

        [FileExtensionLimitation(new string[] { ".png", ".jpg", ".jpeg" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(2 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public new IFormFile? Picture { get; set; }
    }
}
