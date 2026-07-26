using _0_Framework.Application;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.OrderAgg;
using ShopManagement.Infrastructure.EFCore;

namespace PharmaAdmin.Web.Services
{
    public class AutoCancelPendingOrdersJob
    {
        private readonly ShopContext _db;

        public AutoCancelPendingOrdersJob(ShopContext db)
        {
            _db = db;
        }

        [AutomaticRetry(
            Attempts = 3,
            DelaysInSeconds = new[] { 60, 180, 300 },
            OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task ProcessAsync()
        {
            var gatewayTimeout = DateTime.Now.AddHours(-1);

            var abandonedOrders = await _db.Orders
                .Where(o =>
                    (o.Status == OrderStatus.WaitingForGateway && o.LastModifiedDate < gatewayTimeout) ||
                    (o.Status == OrderStatus.Pending && o.LastModifiedDate < gatewayTimeout)
                )
                .ToListAsync();

            if (!abandonedOrders.Any())
                return;

            foreach (var order in abandonedOrders)
            {
                var finalReason =
                    $"[دلیل]: لغو خودکار سیستم{Environment.NewLine}" +
                    $"[توضیحات]: عدم پرداخت یا تکمیل سفارش در زمان مقرر (تایم‌اوت ۱ ساعته){Environment.NewLine}" +
                    $"[توسط]: System (AutoCancelPendingOrdersJob){Environment.NewLine}" +
                    $"[شناسه کاربر]: {order.UserId.ToString().ToPersianNumber()}{Environment.NewLine}" +
                    $"[شناسه سفارش]: {order.Id.ToString().ToPersianNumber()}{Environment.NewLine}" +
                    $"[تاریخ]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";

                order.Cancel(finalReason);
            }

            await _db.SaveChangesAsync();
        }
    }
}
