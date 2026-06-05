namespace ShopManagement.Domain.OrderAgg
{
    public static class PaymentMethod
    {
        public const int None = 0;
        public const int Gateway = 1;
        public const int CardToCard = 2;

        public static string GetMethodByCode(int code)
        {
            return code switch
            {
                0 => "تعیین‌نشده",
                1 => "درگاه پرداخت",
                2 => "کارت به کارت",
                _ => "",
            };
        }
    }
}
