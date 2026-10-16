using _01_PharmaPlusQuery.Contracts.Cart;
using DiscountManagement.Domain.DiscountAgg;
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
            var now = DateTime.Now;

            var cart = await shopContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartKey == cartKey, cancellation);

            if (cart == null)
                return new CartViewModel();

            var productIds = cart.Items.Select(i => i.ProductId).ToList();

            var productInfo = await shopContext.Products
                .Where(x => productIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Picture, x.Name, x.CategoryId, x.Brand, x.Category.FullSlug, x.Slug })
                .ToListAsync(cancellation);

            var inventoryList = await inventoryContext.Inventory
                .Where(x => productIds.Contains(x.ProductId))
                .ToListAsync(cancellation);

            var activeDiscounts = await discountContext.Discounts
                .Include(x => x.Targets)
                .Where(x => x.IsActive && x.Type == (Discount.DiscountType)1
                && x.ApplyType == (Discount.DiscountApplyType)1) // 1: Automatic, 1: OnProduct
                .Where(x => x.StartDate < now && x.EndDate > now)
                .ToListAsync(cancellation);

            var vm = new CartViewModel
            {
                CartKey = cartKey
            };

            double TotalAmount = 0;
            double TotalDiscountAmount = 0;

            foreach (var item in cart.Items)
            {
                var prod = productInfo.FirstOrDefault(x => x.Id == item.ProductId);
                if (prod == null) continue;

                var inv = inventoryList.FirstOrDefault(x => x.ProductId == item.ProductId);
                var bestDiscount = activeDiscounts
                    .Where(d => d.Targets.Any(t =>
                        t.TargetType == (DiscountTarget.DiscountTargetType)4 || // برای همه محصولات
                        (t.TargetType == (DiscountTarget.DiscountTargetType)1 && t.ProductId == item.ProductId) || // برای این محصول خاص
                        (t.TargetType == (DiscountTarget.DiscountTargetType)3 && t.CategoryId == prod.CategoryId) || // برای این دسته بندی
                        (t.TargetType == (DiscountTarget.DiscountTargetType)2 && !string.IsNullOrEmpty(prod.Brand) && t.BrandName?.ToLower() == prod.Brand.ToLower()) // برای این برند
                    ))
                    .OrderByDescending(x => x.Percentage)
                    .FirstOrDefault();

                var invQty = inv?.CalculateCurrentCount() ?? 0;
                var unitPrice = inv?.UnitPrice ?? 0;
                double totalItemAmount = 0;
                double totalItemDiscountAmount = 0;
                double discountedPrice = 0;
                int discountedQty = 0;
                int discountRate = bestDiscount?.Percentage ?? 0;

                if (bestDiscount != null)
                {
                    double discountAmount = (unitPrice * discountRate) / 100;
                    discountedPrice = unitPrice - discountAmount;

                    int maxAllowedInDiscount = (bestDiscount.MaxDiscountQuantity == null || bestDiscount.MaxDiscountQuantity <= 0)
                        ? item.Quantity
                        : bestDiscount.MaxDiscountQuantity.Value;

                    discountedQty = Math.Min(item.Quantity, maxAllowedInDiscount);
                    int normalQty = item.Quantity - discountedQty;

                    totalItemAmount = (discountedQty * discountedPrice) + (normalQty * unitPrice);
                    totalItemDiscountAmount = discountedQty * discountAmount;
                }
                else
                {
                    totalItemAmount = unitPrice * item.Quantity;
                }
                vm.Items.Add(new CartItemViewModel
                {
                    CartKey = cartKey,
                    ProductId = prod.Id,
                    ProductName = prod.Name,
                    ProductFullSlug = $"{prod.FullSlug}/{prod.Slug}",
                    Picture = prod.Picture,
                    Qty = item.Quantity,
                    UnitPrice = unitPrice,
                    PriceWithDiscount = discountedQty > 0 ? discountedPrice : 0,
                    DiscountedQty = discountedQty,
                    InvQty = (int)invQty,
                    DiscountRate = discountRate,
                    HasDiscount = (discountRate > 0 && discountedQty > 0),
                    TotalItemPrice = totalItemAmount
                });

                TotalAmount += unitPrice * item.Quantity;
                TotalDiscountAmount += totalItemDiscountAmount;
            }

            vm.TotalAmount = TotalAmount;
            vm.TotalDiscountAmount = TotalDiscountAmount;
            vm.TotalFinalAmount = TotalAmount - TotalDiscountAmount;

            return vm;
        }
    }
}
