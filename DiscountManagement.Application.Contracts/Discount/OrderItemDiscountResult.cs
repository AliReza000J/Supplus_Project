namespace DiscountManagement.Application.Contracts.Discount
{
    public class OrderItemDiscountResult
    {
        public long DiscountId { get; set; }
        public long ProductId { get; set; }
        public int DiscountRate { get; set; } // درصد تخفیف نهایی محصول
        public double DiscountAmount { get; set; } // مبلغ تخفیف برای این آیتم
        public int DiscountedQty { get; set; }
    }
}
