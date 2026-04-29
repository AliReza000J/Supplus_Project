using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Order
{
    public class OrderTrackingSmsDto
    {
        [Range(1, long.MaxValue)]
        public long OrderId { get; set; }

        private string _postTrackingNo;
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^\d+$", ErrorMessage = "کد رهگیری باید فقط شامل اعداد باشد.")]
        public string PostTrackingNo
        {
            get => _postTrackingNo;
            set => _postTrackingNo = value.ToEnglishNumber();
        }

        private string _mobile;
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string Mobile
        {
            get => _mobile;
            set => _mobile = value.ToEnglishNumber();
        }
    }

}
