using _0_Framework.Application;

namespace DiscountManagement.Application.Contracts.Discount
{
    public class DiscountCalculationModel
    {
        public long UserId { get; set; }

        private string? _couponCode;
        public string? CouponCode
        {
            get => _couponCode;
            set => _couponCode = value?.ToEnglishNumber();
        }

        public List<CartItemForDiscount> Items { get; set; } = new();
    }
}
