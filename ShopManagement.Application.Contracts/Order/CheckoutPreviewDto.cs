namespace ShopManagement.Application.Contracts.Order
{
    public class CheckoutPreviewDto
    {
        public bool IsCouponHasValue { get; set; }

        public string? DiscountMessage { get; set; }

        public bool IsCouponApplied { get; set; }

        public double TotalAmount { get; set; }

        public double TotalDiscountAmount { get; set; }

        public double ShippingCost { get; set; }

        public double FinalPayAmount { get; set; }
    }

}
