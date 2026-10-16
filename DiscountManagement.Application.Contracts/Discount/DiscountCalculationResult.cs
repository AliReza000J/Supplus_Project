using _0_Framework.Application;

namespace DiscountManagement.Application.Contracts.Discount
{
    public class DiscountCalculationResult
    {
        public double TotalAmount { get; set; } = 0;
        public double TotalAutoDiscountAmount { get; set; } = 0;
        public double TotalDiscountAmount { get; set; } = 0;
        public double PayAmount { get; set; } = 0;
        public List<OrderItemDiscountResult> Items { get; set; } = new();

        private string? _couponCode;
        public string? CouponCode
        {
            get => _couponCode;
            set => _couponCode = value?.ToEnglishNumber();
        }

        public long? AppliedCouponId { get; set; }
        public double OrderCouponAmount { get; set; } = 0;
        public int CouponRate { get; set; } = 0;
        public string? DiscountMessage { get; set; }
    }
}
