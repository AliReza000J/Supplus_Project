using System.ComponentModel.DataAnnotations;

namespace DiscountManagement.Application.Contracts.Discount
{
    public class EditDiscount : DefineDiscount
    {
        [Range(1, long.MaxValue, ErrorMessage = "شناسه تخفیف معتبر نیست")]
        public long Id { get; set; }
    }
}
