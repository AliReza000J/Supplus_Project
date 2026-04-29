using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace DiscountManagement.Application.Contracts.Discount
{
    public class DefineDiscount
    {
        [Required(ErrorMessage = "عنوان تخفیف اجباری است")]
        [MaxLength(200, ErrorMessage = "عنوان نباید بیشتر از 255 کاراکتر باشد")]
        public string Title { get; set; }

        private string? _couponCode;
        [MaxLength(50, ErrorMessage = "حداکثر طول کد تخفیف 50 کاراکتر است")]
        public string? CouponCode
        {
            get => _couponCode;
            set => _couponCode = value?.ToEnglishNumber();
        }

        [Range(1, 2, ErrorMessage = "نوع تخفیف نامعتبر است")]
        public int Type { get; set; }

        [Range(1, 2, ErrorMessage = "نحوه اعمال تخفیف نامعتبر است")]
        public int ApplyType { get; set; }

        [Range(1, 100, ErrorMessage = "درصد تخفیف باید بین 1 تا 100 باشد")]
        public int Percentage { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "مبلغ وارد شده معتبر نیست")]
        public double? MinOrderAmount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "تعداد محصول باید حداقل 1 باشد")]
        public int? MaxDiscountQuantity { get; set; }

        private string? _startDate;
        public string? StartDate
        {
            get => _startDate;
            set => _startDate = value?.ToEnglishNumber();
        }

        private string? _endDate;
        public string? EndDate
        {
            get => _endDate;
            set => _endDate = value?.ToEnglishNumber();
        }

        [Range(1, int.MaxValue, ErrorMessage = "ظرفیت کل باید حداقل 1 باشد")]
        public int? MaxUsageCount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "سقف استفاده هر کاربر باید حداقل 1 باشد")]
        public int? UsageLimitPerUser { get; set; }

        public List<long>? ProductIds { get; set; }
        public List<long>? CategoryIds { get; set; }
        public List<string>? BrandNames { get; set; }
    }

}
