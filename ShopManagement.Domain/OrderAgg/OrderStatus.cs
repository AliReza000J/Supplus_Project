namespace ShopManagement.Domain.OrderAgg
{
    public static class OrderStatus
    {
        public const int Pending = 0;           // سفارش ثبت شده ولی هنوز کار خاصی نشده
        public const int WaitingForGateway = 1;   // منتظر پرداخت برای درگاه
        public const int WaitingForManualVerification = 2;   // منتظر پرداخت برای کارت به کارت
        public const int Paid = 3;              // پرداخت موفق
        public const int Processing = 4;        // در حال آماده‌سازی
        public const int Shipped = 5;           // ارسال شده
        public const int Delivered = 6;         // تحویل داده شده به مشتری
        public const int Cancelled = 7;         // لغو شده

        public static string GetStatusTitleByCode(int code)
        {
            return code switch
            {
                0 => "در انتظار",
                1 => "در انتظار پرداخت درگاه",
                2 => "در انتظار تایید پرداخت کارت به کارت",
                3 => "پرداخت شده",
                4 => "در حال پردازش",
                5 => "ارسال شده",
                6 => "تحویل داده شده",
                7 => "لغو شده",
                _ => "",
            };
        }
    }
}
