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

        public async Task<OrderViewModel?> GetDetails(long orderId)
        {
            return await _context.Orders
                .Where(x => x.Id == orderId)
                .Select(x => new OrderViewModel
                {
                    Id = x.Id,
                    AccountId = x.AccountId,
                    DiscountAmount = x.DiscountAmount,
                    RecieverFullName = x.RecieverFullName,
                    RecieverMobile = x.RecieverMobile,
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
                    RecieverAddress = x.RecieverAddress,
                    RecieverZipCode = x.RecieverZipCode,

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
                .FirstOrDefaultAsync();
        }

        public async Task<List<OrderItemViewModel>> GetItems(long orderId)
        {
            var products = await _context.Products.Select(x => new { x.Id, x.Name, x.Picture }).ToListAsync();
            var order = await _context.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == orderId);
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

        public async Task<Order?> GetWithItemsAsync(long orderId)
        {
            return await _context.Orders
                .Include(x => x.Items)
                .Where(x => x.Id == orderId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasThisUserBoughtProduct(long userId, long productId)
        {
            return await _context.Orders
                .AnyAsync(o =>
                    o.AccountId == userId &&
                    o.Items.Any(i => i.ProductId == productId));
        }

        public async Task<List<OrderViewModel>> Search(OrderSearchModel searchModel)
        {
            var query = _context.Orders
                .Select(x => new OrderViewModel
                {
                    Id = x.Id,
                    AccountId = x.AccountId,
                    RecieverFullName = x.RecieverFullName,
                    RecieverMobile = x.RecieverMobile,
                    PaymentMethod = PaymentMethod.GetMethodByCode(x.PayMethod),
                    PaymentMethodInt = x.PayMethod,
                    PayAmount = x.PayAmount,
                    Status = x.Status,
                    StatusTitle = OrderStatus.GetStatusTitleByCode(x.Status),
                    CreationDate = x.CreationDate.ToFarsiFull(),
                    RefNumber = x.PaymentRefId,
                });

            if (searchModel.AccountId > 0) query = query.Where(x => x.AccountId == searchModel.AccountId);

            if (searchModel.PaymentMethod == 1 || searchModel.PaymentMethod == 2)
                query = query.Where(x => x.PaymentMethodInt == searchModel.PaymentMethod);

            if (!string.IsNullOrWhiteSpace(searchModel.Mobile))
                query = query.Where(x => x.RecieverMobile == searchModel.Mobile);

            if (searchModel.Status >= 0 && searchModel.Status != null)
                query = query.Where(x => x.Status == searchModel.Status);

            if (!string.IsNullOrWhiteSpace(searchModel.FromDate))
            {
                var start = searchModel.FromDate.ToGeorgianDateTime();
                query = query.Where(x => x.CreationDate.ToGeorgianDateTime() > start);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ToDate))
            {
                var end = searchModel.ToDate.ToGeorgianDateTime();
                query = query.Where(x => x.CreationDate.ToGeorgianDateTime() < end);
            }

            var orders = await query.OrderByDescending(x => x.Id).ToListAsync();

            return orders;
        }

        public async Task<bool> UserOwnsOrder(string userId, long orderId)
        {
            if (!long.TryParse(userId, out var accountId))
                return false;

            return await _context.Orders
                .AnyAsync(o => o.Id == orderId && o.AccountId == accountId);
        }
    }
}
