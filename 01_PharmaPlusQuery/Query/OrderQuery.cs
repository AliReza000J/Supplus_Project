using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Order;
using AccountManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.OrderAgg;
using ShopManagement.Infrastructure.EFCore;

namespace _01_PharmaPlusQuery.Query
{
    public class OrderQuery : IOrderQuery
    {
        private readonly ShopContext _shopContext;
        private readonly AccountContext _accountContext;

        public OrderQuery(ShopContext orderContext, AccountContext accountContext)
        {
            _shopContext = orderContext;
            _accountContext = accountContext;
        }

        public async Task<OrderQueryModel> GetOrderDetailsAsync(long orderId, long userId, CancellationToken cancellation = default)
        {
            var order = await _shopContext.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Select(o => new OrderQueryModel
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    DiscountAmount = o.DiscountAmount,
                    PayAmount = o.PayAmount,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    PaymentMethodInt = o.PayMethod,
                    PostTrackingNumber = o.ShipTrackingNo,
                    CreationDate = o.CreationDate.ToFarsi(),
                    LastModifiedDate = o.LastModifiedDate.ToFarsiFull(),
                    CouponCode = o.CouponCode,
                    AppliedCouponId = o.AppliedCouponId,
                    OrderCouponAmount = o.OrderCouponAmount,
                    ReceiverAddress = o.ReceiverAddress,
                    ReceiverFullName = o.ReceiverFullName,
                    ReceiverMobile = o.ReceiverMobile,
                    ReceiverZipCode = o.ReceiverZipCode,
                    Items = o.Items.Select(i => new OrderItemQueryModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Qty = i.Quantity,
                        DiscountRate = i.DiscountRate,
                        UnitPrice = i.UnitPrice,
                        ProductPicture = i.Picture,
                        OrderId = i.OrderId,
                        TotalPriceWithDiscount = i.TotalPriceWithDiscount,
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellation);

            if (order == null)
                return null;

            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();

            var productsDict = await _shopContext.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.Category.FullSlug,
                    p.Slug
                })
                .ToDictionaryAsync(p => p.Id, cancellation);

            order.StatusTitle = OrderStatus.GetStatusTitleByCode(order.Status);
            order.PaymentMethod = PaymentMethod.GetMethodByCode(order.PaymentMethodInt);

            foreach (var item in order.Items)
            {
                if (productsDict.TryGetValue(item.ProductId, out var p))
                {
                    item.ProductFullSlug = $"{p.FullSlug}/{p.Slug}";
                }
            }

            return order;
        }

        public async Task<List<OrderSummaryQueryModel>> GetUserOrdersSummaryAsync(long userId, CancellationToken cancellation = default)
        {
            var orders = await _shopContext.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId &&
                            o.Status >= OrderStatus.Pending &&
                            o.Status <= OrderStatus.ReturnRequested)
                .OrderByDescending(o => o.CreationDate)
                .Select(o => new OrderSummaryQueryModel
                {
                    Id = o.Id,
                    PayAmount = o.PayAmount,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreationDate = o.CreationDate.ToFarsi(),
                    StatusTitle = OrderStatus.GetStatusTitleByCode(o.Status),
                    PostTrackingNumber = o.ShipTrackingNo,
                    ItemsCount = o.Items.Count()
                })
                .ToListAsync(cancellation);

            return orders;
        }
    }
}
