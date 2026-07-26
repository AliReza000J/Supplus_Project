using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Account;
using _01_PharmaPlusQuery.Contracts.Order;
using AccountManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.OrderAgg;
using ShopManagement.Infrastructure.EFCore;

namespace _01_PharmaPlusQuery.Query
{
    public class AccountQuery : IAccountQuery
    {
        private readonly ShopContext _shopContext;
        private readonly AccountContext _accountContext;

        public AccountQuery(ShopContext shopContext, AccountContext accountContext)
        {
            _shopContext = shopContext;
            _accountContext = accountContext;
        }

        public async Task<AccountOverviewQueryModel> GetUserOverview(long userId, CancellationToken cancellationToken)
        {
            var baseOrdersQuery = _shopContext.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId);

            var totalOrders = await baseOrdersQuery
                .CountAsync(o => o.Status >= OrderStatus.Pending && o.Status <= OrderStatus.ReturnRequested, cancellationToken);

            var totalRefunded = await baseOrdersQuery
                .CountAsync(o => o.Status == OrderStatus.Refunded, cancellationToken);

            var totalDelivered = await baseOrdersQuery
                .CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);

            var totalFaves = await _shopContext.UserFavoriteProducts
                .AsNoTracking()
                .CountAsync(f => f.UserId == userId, cancellationToken);

            var totalAddresses = await _accountContext.ShippingInfos
                .AsNoTracking()
                .CountAsync(a => a.UserId == userId, cancellationToken);

            var latestOrders = await baseOrdersQuery
                .Where(o => o.Status >= OrderStatus.Pending && o.Status <= OrderStatus.ReturnRequested)
                .OrderByDescending(o => o.CreationDate)
                .Take(3)
                .Select(o => new LatestOrderQueryModel
                {
                    Id = o.Id,
                    CreationDate = o.CreationDate,
                    Status = o.Status,
                    PayAmount = o.PayAmount,
                    ItemsCount = o.Items.Count()
                })
                .ToListAsync(cancellationToken);

            foreach (var order in latestOrders)
            {
                order.CreationDateDisplay = order.CreationDate.ToFarsi();
                order.StatusTitle = OrderStatus.GetStatusTitleByCode(order.Status);
                order.PayAmountDisplay = order.PayAmount.ToMoney();
            }

            return new AccountOverviewQueryModel
            {
                TotalOrders = totalOrders,
                TotalRefundedOrders = totalRefunded,
                TotalDeliveredOrders = totalDelivered,
                TotalFaves = totalFaves,
                TotalAddresses = totalAddresses,
                LatestOrders = latestOrders
            };

        }
    }
}
