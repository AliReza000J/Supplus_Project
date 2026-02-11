using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Order
{
    public class CreateOrderCommand
    {
        public long UserId { get; set; }                 
        public string? ReceiverFullName { get; set; }
        public string? ReceiverMobile { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverZipCode { get; set; }
        public int PayMethod { get; set; } 
        public IFormFile? PaymentReceiptPic { get; set; }
        public string? CardOwnerName { get; set; }        
        public string? NationalCode { get; set; }
    }

    public class CreateOrderCommandDto
    {
        public string? ReceiverFullName { get; set; }

        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string? ReceiverMobile { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string ReceiverAddress { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string ReceiverZipCode { get; set; }

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
