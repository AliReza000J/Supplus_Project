using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Order;
using _01_PharmaPlusQuery.Contracts.Product;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.OrderAgg;
using ShopManagement.Infrastructure.EFCore;

namespace _01_PharmaPlusQuery.Query
{
    public class OrderQuery : IOrderQuery
    {
        private readonly ShopContext _shopContext;

        public OrderQuery(ShopContext orderContext)
        {
            _shopContext = orderContext;
        }

        public async Task<List<OrderQueryModel>> GetOrdersByUser(long userId, CancellationToken cancellation = default)
        {
            var orders = await _shopContext.Orders
                .Where(o => o.UserId == userId &&
                            o.Status >= OrderStatus.Paid &&
                            o.Status <= OrderStatus.Delivered)
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
                    CreationDate = o.CreationDate,
                    LastModifiedDate = o.LastModifiedDate,
                    CouponCode = o.CouponCode,
                    AppliedCouponId = o.AppliedCouponId,
                    OrderCouponAmount = o.OrderCouponAmount,
                    Items = o.Items.Select(i => new OrderItemQueryModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Qty = i.Quantity,
                        DiscountRate = i.DiscountRate,
                        UnitPrice = i.UnitPrice,
                        TotalPriceWithDiscount = i.TotalPriceWithDiscount
                    }).ToList()
                })
                .ToListAsync(cancellation);

            foreach (var order in orders)
            {
                order.DiscountAmountDisplay = order.DiscountAmount.ToMoney();
                order.PayAmountDisplay = order.PayAmount.ToMoney();
                order.OrderCouponAmountDisplay = order.OrderCouponAmount.ToMoney();
                order.TotalAmountDisplay = order.TotalAmount.ToMoney();

                order.StatusTitle = OrderStatus.GetStatusTitleByCode(order.Status);
                order.PaymentMethod = PaymentMethod.GetMethodByCode(order.PaymentMethodInt);

                order.CreationDateDisplay = order.CreationDate.ToFarsi();
                order.LastModifiedDateDisplay = order.LastModifiedDate.ToFarsiFull();

                foreach (var item in order.Items)
                {
                    item.UnitPriceDisplay = item.UnitPrice.ToMoney();
                    item.TotalPriceWithDiscountDisplay = item.TotalPriceWithDiscount.ToMoney();
                    item.DiscountRateDisplay = item.DiscountRate.ToString() + "%";
                }
            }


            var orderedProductIds = orders
                .SelectMany(o => o.Items)
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var products = await _shopContext.Products
                .Where(x => orderedProductIds.Contains(x.Id))
                .Select(product => new ProductQueryModel
                {
                    Id = product.Id,
                    CategoryName = product.Category.Name,
                    CategoryFullSlug = product.Category.FullSlug,
                    Slug = product.Slug,
                })
                .ToListAsync(cancellation);

            var productDict = products.ToDictionary(x => x.Id);

            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    if (productDict.TryGetValue(item.ProductId, out var p))
                        item.ProductFullPath = $"{p.CategoryFullSlug}/{p.Slug}";
                }
            }

            return orders.OrderByDescending(x => x.SortDate).ToList();
        }
    }
}
