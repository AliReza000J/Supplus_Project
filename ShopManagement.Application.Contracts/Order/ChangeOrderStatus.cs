namespace ShopManagement.Application.Contracts.Order
{
    public class ChangeOrderStatus
    {
        public long OrderId { get; set; }
        public OrderStatus Status { get; set; }
    }
    public enum OrderStatus
    {
        Pending = 0,           // سفارش ثبت شده ولی هنوز کار خاصی نشده
        AwaitingPayment = 1,   // منتظر پرداخت (برای کارت به کارت)
        Paid = 2,              // پرداخت موفق
        Processing = 3,        // در حال آماده‌سازی
        Shipped = 4,           // ارسال شده
        Delivered = 5,         // تحویل داده شده به مشتری
        Cancelled = 6          // لغو شده
    }
}