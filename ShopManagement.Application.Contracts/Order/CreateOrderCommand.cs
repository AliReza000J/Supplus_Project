using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Order
{
    public class CreateOrderCommand
    {
        public long AccountId { get; set; }                 // must be provided (user must be logged in for real checkout)

        public string? RecieverFullName { get; set; }

        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string? RecieverMobile { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string RecieverAddress { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string RecieverZipCode { get; set; }

        [Range(1, 2, ErrorMessage = ValidationMessages.InvalidInput)]
        public int PayMethod { get; set; }      // e.g. "Online", "COD", "BankTransfer"

        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(2 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public IFormFile? PaymentReceiptPic { get; set; } // رسید پرداخت برای کارت به کارت (فایل تصویر)

        public string? CardOwnerName { get; set; } // نام صاحب کارت برای کارت به کارت
        
        [RegularExpression(@"^\d{10}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string? NationalCode { get; set; }  // کد ملی صاحب کارت
    }
}
