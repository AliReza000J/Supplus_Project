using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Order
{
    /// <summary>
    /// Data Transfer Object containing the details required to place a new order.
    /// </summary>
    public class CreateOrderCommandDto
    {
        /// <summary>
        /// The full name of the person receiving the order.
        /// </summary>
        [Display(Name = "نام و نام‌ خانوادگی گیرنده")]
        public string? ReceiverFullName { get; set; }

        private string? _receiverMobile;
        /// <summary>
        /// The mobile phone number of the receiver. 
        /// Must be in a valid Iranian mobile format (e.g., 09123456789).
        /// </summary>
        [Display(Name = "شماره موبایل گیرنده")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string? ReceiverMobile
        {
            get => _receiverMobile;
            set => _receiverMobile = value?.ToEnglishNumber();
        }

        /// <summary>
        /// The physical shipping address where the order will be delivered.
        /// </summary>
        [Display(Name = "نشانی گیرنده")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string ReceiverAddress { get; set; }

        private string _receiverZipCode;
        /// <summary>
        /// The 10-digit postal code (zip code) for the delivery address.
        /// </summary>
        [Display(Name = "کد پستی")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string ReceiverZipCode
        {
            get => _receiverZipCode;
            set => _receiverZipCode = value?.ToEnglishNumber();
        }

        /// <summary>
        /// The selected payment method for the order.
        /// Valid values typically include 1 (Online Gateway) or 2 (Bank Transfer/Offline).
        /// </summary>
        [Range(1, 2, ErrorMessage = ValidationMessages.InvalidInput)]
        public int PayMethod { get; set; }

        /// <summary>
        /// An optional image of the payment receipt. 
        /// Typically used and required when the payment method is set to Bank Transfer.
        /// Allowed extensions: .jpeg, .jpg, .png. Maximum file size: 2MB.
        /// </summary>
        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(2 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public IFormFile? PaymentReceiptPic { get; set; }

        private string? _couponCode;
        /// <summary>
        /// An optional discount coupon code to be applied to the order.
        /// </summary>
        [Display(Name = "کد تخفیف")]
        public string? CouponCode
        {
            get => _couponCode;
            set => _couponCode = value?.ToEnglishNumber();
        }

        /// <summary>
        /// The name of the bank card owner. 
        /// Used for verifying offline payments (Bank Transfer).
        /// </summary>
        [Display(Name = "نام و نام خانوادگی صاحب کارت")]
        public string? CardOwnerName { get; set; }

        private string? _nationalCode;
        /// <summary>
        /// The 10-digit National Identity Code of the card owner.
        /// Used for verifying offline payments (Bank Transfer).
        /// </summary>
        [Display(Name = "کد ملی صاحب کارت")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string? NationalCode
        {
            get => _nationalCode;
            set => _nationalCode = value?.ToEnglishNumber();
        }
    }

}
