using System.ComponentModel.DataAnnotations;

namespace CommentManagement.Application.Contracts.Comment
{
    public class EditComment
    {
        [Required]
        public long Id { get; set; }

        // هر دو فیلد اختیاری هستن چون ممکنه فقط یکی ویرایش بشه
        public string? Message { get; set; }

        [Range(1, 5, ErrorMessage = "امتیاز باید بین ۱ تا ۵ باشد.")]
        public int Rate { get; set; }

        //// مخصوص ادمین (در صورت ویرایش پاسخ)
        //public string? Reply { get; set; }
    }
}
