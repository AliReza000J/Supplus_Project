namespace ShopManagement.Domain.OrderAgg
{
    public static class OrderStatus
    {
        public const int Pending = 1;
        public const int WaitingForGateway = 2;
        public const int WaitingForManualVerification = 3;
        public const int Paid = 4;
        public const int Processing = 5;
        public const int Shipped = 6;
        public const int Delivered = 7;
        public const int NeedsRefund = 8;
        public const int Cancelled = 9;
        public const int Refunded = 10;
        public const int ReturnRequested = 11;

        public static string GetStatusTitleByCode(int code)
        {
            return code switch
            {
                1 => "ثبت شده، پرداخت نشده",
                2 => "در انتظار پرداخت درگاه",
                3 => "در انتظار تایید کارت به کارت",
                4 => "پرداخت موفق",
                5 => "در حال آماده‌سازی",
                6 => "ارسال شده",
                7 => "تحویل داده شده",
                8 => "نیازمند بازگشت وجه",
                9 => "لغو شده",
                10 => "مرجوع شده",
                11 => "درخواست مرجوعی",
                _ => "",
            };
        }
    }
}
