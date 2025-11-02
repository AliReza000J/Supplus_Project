using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace CommentManagement.Application.Contracts.Comment
{
    public class AddComment
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Username { get; set; }

        [EmailAddress(ErrorMessage = "ایمیل نامعتبر است.")]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Message { get; set; }

        [Required]
        public long OwnerRecordId { get; set; }

        [Required]
        public int Type { get; set; }
        //public long ParentId { get; set; }


        [Required]
        [Range(1, 5, ErrorMessage = "امتیاز باید بین ۱ تا ۵ باشد.")]
        public int Rate { get; set; }
    }
}
