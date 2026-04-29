using _0_Framework.Application;
using _0_Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Application.Contracts.Order;
using ShopManagement.Domain.OrderAgg;

namespace ShopManagement.Infrastructure.EFCore.Repository
{
    public class OrderRepository : BaseRepository<long, Order>, IOrderRepository
    {
        private readonly ShopContext _context;
        public OrderRepository(ShopContext context) : base(context)
        {
            _context = context;
        }

        public async Task<OrderViewModel?> GetDetails(long orderId, CancellationToken cancellation)
        {
            return await _context.Orders
                .Where(x => x.Id == orderId)
                .Select(x => new OrderViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    DiscountAmount = x.DiscountAmount,
                    ReceiverFullName = x.ReceiverFullName,
                    ReceiverMobile = x.ReceiverMobile,
                    PostTrackingNumber = x.ShipTrackingNo ?? "",
                    PaymentMethod = PaymentMethod.GetMethodByCode(x.PayMethod),
                    PaymentMethodInt = x.PayMethod,
                    PayAmount = x.PayAmount,
                    Status = x.Status,
                    StatusTitle = OrderStatus.GetStatusTitleByCode(x.Status),
                    TotalAmount = x.TotalAmount,
                    CreationDate = x.CreationDate.ToFarsi(),
                    RefNumber = x.PaymentRefId,
                    CardOwnerFullname = x.CardOwnerFullname,
                    CardOwnerNationalCode = x.CardOwnerNationalCode,
                    PaymentReceiptPic = x.PaymentReceiptPic,
                    CanellationReason = x.CancellationReason,
                    LastModifiedDate = x.LastModifiedDate.ToFarsiFull(),
                    ReceiverAddress = x.ReceiverAddress,
                    ReceiverZipCode = x.ReceiverZipCode,
                    CouponCode = x.CouponCode,
                    AppliedCouponId = x.AppliedCouponId,
                    OrderCouponAmount = x.OrderCouponAmount,

                    Items = x.Items.Select(i => new OrderItemViewModel
                    {
                        Id = i.Id,
                        Qty = i.Quantity,
                        DiscountRate = i.DiscountRate,
                        OrderId = i.OrderId,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductPicture = i.Picture,
                        TotalDiscountAmount = i.TotalDiscountAmount,
                        TotalPriceWithDiscount = i.TotalPriceWithDiscount,
                        UnitPrice = i.UnitPrice,
                        DiscountedQty = i.DiscountedQty
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellation);
        }

        public async Task<List<OrderItemViewModel>> GetItems(long orderId, CancellationToken cancellation)
        {
            var products = await _context.Products.Select(x => new { x.Id, x.Name, x.Picture }).ToListAsync(cancellation);
            var order = await _context.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == orderId, cancellation);
            if (order == null)
                return new List<OrderItemViewModel>();

            var items = order.Items.Select(x => new OrderItemViewModel
            {
                Id = x.Id,
                Qty = x.Quantity,
                DiscountRate = x.DiscountRate,
                OrderId = x.OrderId,
                ProductId = x.ProductId,
                UnitPrice = x.UnitPrice,
                TotalPriceWithDiscount = x.TotalPriceWithDiscount,
                TotalDiscountAmount = x.TotalDiscountAmount,
                DiscountedQty = x.DiscountedQty,
                ProductName = x.ProductName,
                ProductPicture = x.Picture
            }).ToList();

            //foreach (var item in items)
            //{
            //    item.ProductName = products.FirstOrDefault(x => x.Id == item.ProductId)?.Name ?? "";
            //    item.ProductPicture = products.FirstOrDefault(x => x.Id == item.ProductId)?.Picture ?? "";
            //}

            return items;
        }

        public async Task<Order?> GetWithItemsAsync(long orderId, CancellationToken cancellation)
        {
            return await _context.Orders
                .Include(x => x.Items)
                .Where(x => x.Id == orderId)
                .FirstOrDefaultAsync(cancellation);
        }

        public async Task<bool> HasThisUserBoughtProduct(long userId, long productId, CancellationToken cancellation)
        {
            return await _context.Orders
                .AnyAsync(o =>
                    o.UserId == userId &&
                    o.Items.Any(i => i.ProductId == productId), cancellation);
        }

        public async Task<List<OrderViewModel>> Search(OrderSearchModel searchModel, CancellationToken cancellation)
        {
            var query = _context.Orders.AsQueryable();

            if (searchModel.UserId > 0) query = query.Where(x => x.UserId == searchModel.UserId);

            if (searchModel.PaymentMethod == 1 || searchModel.PaymentMethod == 2)
                query = query.Where(x => x.PayMethod == searchModel.PaymentMethod);

            if (!string.IsNullOrWhiteSpace(searchModel.Mobile))
                query = query.Where(x => x.ReceiverMobile == searchModel.Mobile);

            if (searchModel.Status >= 0 && searchModel.Status != null)
                query = query.Where(x => x.Status == searchModel.Status);
            else
                query = query.Where(x => x.Status >= OrderStatus.WaitingForManualVerification);

            if (!string.IsNullOrWhiteSpace(searchModel.FromDate))
            {
                var start = searchModel.FromDate.ToGregorianDateTime();
                query = query.Where(x => x.CreationDate > start);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ToDate))
            {
                var end = searchModel.ToDate.ToGregorianDateTime();
                query = query.Where(x => x.CreationDate < end);
            }
            var orders = await query
                .OrderByDescending(x => x.Id)
                .Select(x => new OrderViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    ReceiverFullName = x.ReceiverFullName,
                    ReceiverMobile = x.ReceiverMobile,
                    PaymentMethod = PaymentMethod.GetMethodByCode(x.PayMethod),
                    PaymentMethodInt = x.PayMethod,
                    PayAmount = x.PayAmount,
                    Status = x.Status,
                    StatusTitle = OrderStatus.GetStatusTitleByCode(x.Status),
                    CreationDate = x.CreationDate.ToFarsiFull(),
                    RefNumber = x.PaymentRefId,
                })
                .ToListAsync(cancellation);

            return orders;
        }

        public async Task<bool> UserOwnsOrder(long userId, long orderId, CancellationToken cancellation)
        {
            if (userId <= 0)
                return false;

            return await _context.Orders
                .AnyAsync(o => o.Id == orderId && o.UserId == userId, cancellation);
        }
    }
}
