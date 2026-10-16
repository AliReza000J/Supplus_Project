namespace ShopManagement.Application.Contracts.Order
{
    public class CheckoutPreviewDto
    {
        public double TotalAmount { get; set; } = 0;

        public bool IsCouponHasValue { get; set; }

        public string? DiscountMessage { get; set; }

        public bool IsCouponApplied { get; set; }

        public double OrderCouponAmount { get; set; } = 0;
        public int CouponRate { get; set; } = 0;

        public double TotalAutoDiscountAmount { get; set; } = 0;

        public double TotalDiscountAmount { get; set; } = 0;

        public double ShippingCost { get; set; } = 0;

        public double FinalPayAmount { get; set; } = 0;
    }
}
