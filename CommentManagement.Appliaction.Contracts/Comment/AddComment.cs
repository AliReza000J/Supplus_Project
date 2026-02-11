using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace CommentManagement.Application.Contracts.Comment
{
    public class AddComment
    {
        public string? Message { get; set; }

        [Required]
        public long RecordId { get; set; }

        [Required]
        public int Type { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "امتیاز باید بین ۱ تا ۵ باشد.")]
        public int Rate { get; set; }
    }
}
