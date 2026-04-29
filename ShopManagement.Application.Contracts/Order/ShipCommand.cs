using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Order
{
    public class ShipCommand
    {
        public long OrderId { get; set; }

        private string _postTrackingNo;
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^\d+$", ErrorMessage = "کد رهگیری باید فقط شامل اعداد باشد.")]
        public string PostTrackingNo
        {
            get => _postTrackingNo;
            set => _postTrackingNo = value?.ToEnglishNumber();
        }

        private string _mobile;
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Mobile
        {
            get => _mobile;
            set => _mobile = value?.ToEnglishNumber();
        }
    }
}
