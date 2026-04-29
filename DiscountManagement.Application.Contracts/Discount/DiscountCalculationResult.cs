using _0_Framework.Application;

namespace DiscountManagement.Application.Contracts.Discount
{
    public class DiscountCalculationResult
    {
        public double TotalAmount { get; set; }
        public double TotalDiscountAmount { get; set; }
        public double PayAmount { get; set; }
        public List<OrderItemDiscountResult> Items { get; set; } = new();

        private string? _couponCode;
        public string? CouponCode
        {
            get => _couponCode;
            set => _couponCode = value?.ToEnglishNumber();
        }

        public long? AppliedCouponId { get; set; }
        public double OrderCouponAmount { get; set; }
        public string? DiscountMessage { get; set; }
    }
}
