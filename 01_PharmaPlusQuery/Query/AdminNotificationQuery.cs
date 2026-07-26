using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.AdminNotification;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.OrderAgg;
using ShopManagement.Infrastructure.EFCore;
using SiteManagement.Infrastructure.EFCore;

namespace _01_PharmaPlusQuery.Query
{
    public class AdminNotificationQuery : IAdminNotificationQuery
    {
        private readonly ShopContext _shopContext;
        private readonly SiteContext _siteContext;

        public AdminNotificationQuery(SiteContext siteContext, ShopContext shopContext)
        {
            _siteContext = siteContext;
            _shopContext = shopContext;
        }

        public async Task<AdminNotificationDto> GetAdminNotifications()
        {
            var twoWeeksAgo = DateTime.Now.AddDays(-14);

            var newTickets = await _siteContext.Tickets.AsNoTracking()
                .CountAsync(x => !x.IsAnswered && x.CreationDate >= twoWeeksAgo);

            var pendingOrders = await _shopContext.Orders.AsNoTracking()
                .CountAsync(x => x.Status >= OrderStatus.Pending
                              && x.Status <= OrderStatus.Paid
                              && x.CreationDate >= twoWeeksAgo);

            var refundRequests = await _shopContext.Orders.AsNoTracking()
                .CountAsync(x => x.Status == OrderStatus.NeedsRefund || x.Status == OrderStatus.ReturnRequested);

            var recentTickets = await _siteContext.Tickets.AsNoTracking()
                .Where(x => !x.IsAnswered && x.CreationDate >= twoWeeksAgo)
                .OrderByDescending(x => x.CreationDate)
                .Take(3)
                .Select(x => new NotificationItemDto
                {
                    Id = x.Id,
                    Title = "تیکت جدید",
                    Message = x.Subject,
                    CreationDate = x.CreationDate.ToFarsiFull(),
                    IconClass = "fa fa-ticket",
                    ColorClass = "text-warning",
                    Link = $"/Admin/Shop/Tickets/Index"
                })
                .ToListAsync();

            var recentOrders = await _shopContext.Orders.AsNoTracking()
                .Where(x => ((x.Status >= OrderStatus.Pending && x.Status <= OrderStatus.Paid)
                          || x.Status == OrderStatus.NeedsRefund
                          || x.Status == OrderStatus.ReturnRequested) // اضافه شدن وضعیت 11
                         && x.CreationDate >= twoWeeksAgo)
                .OrderByDescending(x => x.CreationDate)
                .Take(3)
                .Select(x => new NotificationItemDto
                {
                    Id = x.Id,
                    Title = x.Status == OrderStatus.ReturnRequested ? "درخواست مرجوعی جدید" :
                            x.Status == OrderStatus.NeedsRefund ? "نیازمند بازگشت وجه" : "سفارش جدید",
                    Message = $"سفارش شماره {x.Id}",
                    CreationDate = x.CreationDate.ToFarsiFull(),
                    IconClass = x.Status == OrderStatus.ReturnRequested ? "fa fa-exchange" :
                                x.Status == OrderStatus.NeedsRefund ? "fa fa-undo" : "fa fa-shopping-cart",
                    ColorClass = x.Status == OrderStatus.ReturnRequested ? "text-primary" :
                                 x.Status == OrderStatus.NeedsRefund ? "text-danger" : "text-success",
                    Link = $"/Admin/Shop/Orders/Details/{x.Id}"
                })
                .ToListAsync();

            var combinedRecentNotifications = recentTickets
                .Concat(recentOrders)
                .ToList();

            var dto = new AdminNotificationDto
            {
                NewTicketsCount = newTickets,
                PendingOrdersCount = pendingOrders,
                RefundRequestsCount = refundRequests,
                RecentNotifications = combinedRecentNotifications
            };

            return dto;
        }


        public async Task<List<NotificationItemDto>> GetAllAdminNotifications()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var tickets = await _siteContext.Tickets.AsNoTracking()
                .Where(x => !x.IsAnswered && x.CreationDate >= sixMonthsAgo)
                .ToListAsync();

            var ticketDtos = tickets.Select(x => new NotificationItemDto
            {
                Id = x.Id,
                Title = "تیکت جدید",
                Message = x.Subject,
                CreationDate = x.CreationDate.ToFarsiFull(),
                ActualDate = x.CreationDate,
                IconClass = "fa fa-ticket",
                ColorClass = "text-warning",
                Link = $"/Admin/Shop/Tickets/Index",
            }).ToList();

            var orders = await _shopContext.Orders.AsNoTracking()
                .Where(x => ((x.Status >= OrderStatus.Pending && x.Status <= OrderStatus.Paid)
                          || x.Status == OrderStatus.NeedsRefund
                          || x.Status == OrderStatus.ReturnRequested) // اضافه شدن وضعیت 11
                         && x.CreationDate >= sixMonthsAgo)
                .ToListAsync();

            var orderDtos = orders.Select(x => new NotificationItemDto
            {
                Id = x.Id,
                Title = x.Status == OrderStatus.ReturnRequested ? "درخواست مرجوعی جدید" :
                        x.Status == OrderStatus.NeedsRefund ? "نیازمند بازگشت وجه" : "سفارش جدید",
                Message = $"سفارش شماره {x.Id}",
                CreationDate = x.CreationDate.ToFarsiFull(),
                ActualDate = x.CreationDate,
                IconClass = x.Status == OrderStatus.ReturnRequested ? "fa fa-exchange" :
                            x.Status == OrderStatus.NeedsRefund ? "fa fa-undo" : "fa fa-shopping-cart",
                ColorClass = x.Status == OrderStatus.ReturnRequested ? "text-primary" :
                             x.Status == OrderStatus.NeedsRefund ? "text-danger" : "text-success",
                Link = $"/Admin/Shop/Orders/Details/{x.Id}"
            }).ToList();

            var combinedNotifications = ticketDtos.Concat(orderDtos).ToList();

            return combinedNotifications.OrderByDescending(x => x.ActualDate).ToList();
        }
    }
}
