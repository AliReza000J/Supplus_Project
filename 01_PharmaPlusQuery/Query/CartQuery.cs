using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Cart;
using DiscountManagement.Infrastructure.EFCore;
using InventoryMangement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.EFCore;

namespace _01_PharmaPlusQuery.Query
{
    public class CartQuery : ICartQuery
    {
        private readonly ShopContext shopContext;
        private readonly DiscountContext discountContext;
        private readonly InventoryContext inventoryContext;

        public CartQuery(ShopContext shopContext, DiscountContext discountContext, InventoryContext inventoryContext)
        {
            this.shopContext = shopContext;
            this.discountContext = discountContext;
            this.inventoryContext = inventoryContext;
        }

        public async Task<CartViewModel> GetCartAsync(string cartKey, CancellationToken cancellation = default)
        {
            var cart = await shopContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartKey == cartKey, cancellation);

            if (cart == null)
                return new CartViewModel();

            var productIds = cart.Items.Select(i => i.ProductId).ToList();

            var inventoryList = await inventoryContext.Inventory
                .Where(x => productIds.Contains(x.ProductId))
                .ToListAsync(cancellation);

            var discountList = await discountContext.CustomerDiscounts
                .Where(x => productIds.Contains(x.ProductId) &&
                            x.StartDate <= DateTime.Now &&
                            x.EndDate >= DateTime.Now)
                .ToListAsync(cancellation);

            var productInfo = await shopContext.Products
                .Where(x => productIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Picture, x.Name })
                .ToListAsync(cancellation);

            var vm = new CartViewModel
            {
                CartKey = cartKey
            };

            double TotalAmount = 0;

            foreach (var item in cart.Items)
            {
                var inv = inventoryList.FirstOrDefault(x => x.ProductId == item.ProductId);
                var disc = discountList.FirstOrDefault(x => x.ProductId == item.ProductId);
                var prod = productInfo.FirstOrDefault(x => x.Id == item.ProductId);

                var invQty = (inv != null) ? inv.CalculateCurrentCount() : 0;
                var unitPrice = inv?.UnitPrice ?? 0;
                double totalItemAmount = 0;
                double finalUnitPrice = 0;
                int discountedQty = 0;

                if (disc != null)
                {
                    double discountAmount = (unitPrice * disc.DiscountRate) / 100;
                    double discountedPrice = unitPrice - discountAmount;

                    int maxDiscountQty = disc.ProductCount <= 0 ? item.Quantity : disc.ProductCount;
                    discountedQty = Math.Min(item.Quantity, maxDiscountQty);
                    int normalQty = item.Quantity - discountedQty;

                    totalItemAmount =
                        discountedQty * discountedPrice +
                        normalQty * unitPrice;

                    finalUnitPrice = discountedPrice;
                }
                else
                {
                    totalItemAmount = unitPrice * item.Quantity;
                }
                var hasDisc = disc != null && disc.DiscountRate > 0;
                vm.Items.Add(new CartItemViewModel
                {
                    CartKey = cartKey,
                    ProductId = prod.Id,
                    ProductName = prod.Name,
                    Picture = prod.Picture,
                    Qty = item.Quantity,
                    UnitPrice = unitPrice.ToMoney().ToPersianNumber(),
                    PriceWithDiscount = discountedQty > 0 ? finalUnitPrice.ToMoney().ToPersianNumber() : string.Empty,
                    DiscountedQty = discountedQty,
                    InvQty = (int)invQty,
                    DiscountRate = hasDisc ? disc.DiscountRate : 0,
                    HasDiscount = hasDisc
                });

                TotalAmount += totalItemAmount;
            }

            vm.TotalAmount = TotalAmount.ToMoney().ToPersianNumber();

            return vm;
        }
    }
}
