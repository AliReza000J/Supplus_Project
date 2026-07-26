using _0_Framework.Application;
using DiscountManagement.Application.Contracts.Discount;
using DiscountManagement.Domain.DiscountAgg;
using static DiscountManagement.Domain.DiscountAgg.Discount;

namespace DiscountManagement.Application
{
    public class DiscountApplication : IDiscountApplication
    {
        private readonly IDiscountRepository _discountRepository;

        public DiscountApplication(IDiscountRepository discountRepository)
        {
            _discountRepository = discountRepository;
        }

        public async Task<DiscountCalculationResult> CalculateDiscountsAsync(DiscountCalculationModel command, CancellationToken cancellation)
        {
            var result = new DiscountCalculationResult();
            var now = DateTime.Now;

            var productIds = command.Items.Select(x => x.ProductId).ToList();
            var categoryIds = command.Items.Select(x => x.CategoryId).Distinct().ToList();
            var brands = command.Items.Select(x => x.Brand).Distinct().ToList();

            var activeAutoDiscounts = await _discountRepository.GetActiveAutomaticDiscounts(
                productIds, categoryIds, brands, cancellation);

            Discount? coupon = null;
            if (!string.IsNullOrWhiteSpace(command.CouponCode))
            {
                coupon = await _discountRepository.GetByCouponCode(command.CouponCode, cancellation);

                if (coupon == null)
                {
                    result.DiscountMessage = "کد تخفیف وارد شده نامعتبر است.";
                }
                else if (!coupon.IsUsable(command.UserId, now))
                {
                    result.DiscountMessage = "این کد تخفیف منقضی شده یا ظرفیت استفاده از آن به پایان رسیده است.";
                    coupon = null;
                }
            }

            double totalAmount = 0;
            double totalProductDiscount = 0;

            foreach (var cartItem in command.Items)
            {
                var itemOriginalPrice = cartItem.UnitPrice * cartItem.Quantity;
                totalAmount += itemOriginalPrice;

                var bestAutoDiscount = activeAutoDiscounts
                    .Where(d => d.IsProductDiscount &&
                    d.AppliesToProduct(cartItem.ProductId, cartItem.CategoryId, cartItem.Brand))
                    .OrderByDescending(d => d.Percentage) // بیشترین درصد را اول می‌آوریم
                    .FirstOrDefault();

                bool couponAppliesToThis = coupon != null &&
                                   coupon.ApplyType == DiscountApplyType.OnProduct &&
                                   coupon.AppliesToProduct(cartItem.ProductId, cartItem.CategoryId, cartItem.Brand);

                Discount? selectedDiscount = bestAutoDiscount;
                if (couponAppliesToThis)
                {
                    if (bestAutoDiscount == null || coupon!.Percentage > bestAutoDiscount.Percentage)
                    {
                        selectedDiscount = coupon;
                        result.AppliedCouponId = coupon!.Id; // ثبت استفاده از کوپن
                    }
                }

                double itemDiscountAmount = 0;
                int finalRate = 0;
                long finalDiscountId = 0;
                int discountedQty = 0;

                if (selectedDiscount != null)
                {
                    finalRate = selectedDiscount.Percentage;
                    finalDiscountId = selectedDiscount.Id;

                    var maxQty = selectedDiscount.MaxDiscountQuantity ?? 0;

                    discountedQty = (maxQty <= 0)
                        ? cartItem.Quantity
                        : Math.Min(cartItem.Quantity, maxQty);

                    itemDiscountAmount = (cartItem.UnitPrice * finalRate / 100) * discountedQty;
                }

                totalProductDiscount += itemDiscountAmount;

                result.Items.Add(new OrderItemDiscountResult
                {
                    ProductId = cartItem.ProductId,
                    DiscountRate = finalRate,
                    DiscountAmount = itemDiscountAmount,
                    DiscountId = finalDiscountId,
                    DiscountedQty = discountedQty
                });
            }

            result.TotalAmount = totalAmount;
            result.TotalDiscountAmount = totalProductDiscount;

            var amountAfterProductDiscounts = totalAmount - totalProductDiscount;

            if (coupon != null)
            {
                if (coupon.IsOrderDiscount)
                {
                    if (coupon.MinOrderAmount.HasValue && amountAfterProductDiscounts < (double)coupon.MinOrderAmount.Value)
                    {
                        result.DiscountMessage = $"این کد برای سفارش‌های بالای {coupon.MinOrderAmount.Value.ToMoney()} تومان است.";
                    }
                    else
                    {
                        var orderCouponAmount = (amountAfterProductDiscounts * coupon.Percentage) / 100;
                        result.OrderCouponAmount = orderCouponAmount;
                        result.TotalDiscountAmount += orderCouponAmount;
                        result.AppliedCouponId = coupon.Id;
                        result.CouponCode = coupon.CouponCode;
                        result.DiscountMessage = "کد تخفیف با موفقیت روی سبد خرید شما اعمال شد.";
                    }
                }
                else if (coupon.IsProductDiscount)
                {
                    if (result.AppliedCouponId == coupon.Id)
                        result.DiscountMessage = "کد تخفیف روی محصولات منتخب اعمال شد.";
                    else
                        result.DiscountMessage = "این کد تخفیف شامل محصولات موجود در سبد خرید شما نمی‌شود.";
                }
            }

            result.PayAmount = result.TotalAmount - result.TotalDiscountAmount;
            return result;
        }
        public async Task<OperationResult> Define(DefineDiscount command, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            if (command.Type == 2) // Coupon
            {
                if (string.IsNullOrWhiteSpace(command.CouponCode))
                    return operation.Failed("برای تخفیف‌های نوع کوپنی، وارد کردن کد کوپن الزامی است.");

                if (await _discountRepository.ExistsAsync(x => x.CouponCode == command.CouponCode, cancellation))
                    return operation.Failed("این کد تخفیف قبلاً در سیستم ثبت شده است.");

                command.CouponCode = command.CouponCode;
            }
            else
            {
                command.CouponCode = null;
            }

            DateTime? startDate = command.StartDate?.ToGregorianDateTime();
            DateTime? endDate = command.EndDate?.ToGregorianDateTime();

            if (startDate.HasValue && endDate.HasValue && startDate >= endDate)
                return operation.Failed("تاریخ شروع نمی‌تواند بزرگتر یا مساوی تاریخ پایان باشد.");

            if (endDate.HasValue && endDate.Value < DateTime.Now)
                return operation.Failed("تاریخ پایان تخفیف نمی‌تواند در گذشته باشد.");

            var discount = new Discount(
                command.Title,
                command.CouponCode?.ToUpper(),
                (Discount.DiscountType)command.Type,
                (Discount.DiscountApplyType)command.ApplyType,
                command.Percentage,
                command.MinOrderAmount,
                command.MaxDiscountQuantity,
                startDate,
                endDate,
                command.MaxUsageCount,
                command.UsageLimitPerUser);

            if (command.ApplyType == 2) // OnWholeOrder
            {
                discount.AddTarget(DiscountTarget.ForAll());
            }
            else
            {
                AddDiscountTargets(discount, command);
            }

            _discountRepository.Create(discount);
            await _discountRepository.SaveChangesAsync(cancellation);

            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditDiscount command, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            var discount = await _discountRepository.GetWithTargetsAsync(command.Id, cancellation);
            if (discount == null)
                return operation.Failed("رکورد مورد نظر یافت نشد.");

            if (command.Type == 2) // Coupon
            {
                if (string.IsNullOrWhiteSpace(command.CouponCode))
                    return operation.Failed("برای تخفیف‌های نوع کوپنی، وارد کردن کد کوپن الزامی است.");

                if (await _discountRepository.ExistsAsync(x => x.CouponCode == command.CouponCode && x.Id != command.Id, cancellation))
                    return operation.Failed("این کد تخفیف قبلاً برای تخفیف دیگری ثبت شده است.");

                command.CouponCode = command.CouponCode;
            }
            else
            {
                command.CouponCode = null;
            }

            DateTime? startDate = command.StartDate?.ToGregorianDateTime();
            DateTime? endDate = command.EndDate?.ToGregorianDateTime();

            if (startDate.HasValue && endDate.HasValue && startDate >= endDate)
                return operation.Failed("تاریخ شروع نمی‌تواند بزرگتر یا مساوی تاریخ پایان باشد.");

            discount.Edit(
                command.Title,
                command.CouponCode?.ToUpper(),
                (Discount.DiscountType)command.Type,
                (Discount.DiscountApplyType)command.ApplyType,
                command.Percentage,
                command.MinOrderAmount,
                command.MaxDiscountQuantity,
                startDate,
                endDate,
                command.MaxUsageCount,
                command.UsageLimitPerUser);

            discount.ClearTargets();

            if (command.ApplyType == 2) // OnWholeOrder
            {
                discount.AddTarget(DiscountTarget.ForAll());
            }
            else
            {
                AddDiscountTargets(discount, command);
            }

            await _discountRepository.SaveChangesAsync(cancellation);

            return operation.Succeeded();
        }

        public async Task<EditDiscount?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _discountRepository.GetDetails(id, cancellation);
        }

        public async Task IncreaseUsage(long discountId, long userId, CancellationToken cancellation)
        {
            var discount = await _discountRepository.GetWithTargetsAsync(discountId, cancellation);
            if (discount != null)
            {
                discount.IncreaseUsage(userId);
                await _discountRepository.SaveChangesAsync(cancellation);
            }
        }

        public async Task<List<DiscountViewModel>> Search(DiscountSearchModel searchModel, CancellationToken cancellation)
        {
            return await _discountRepository.Search(searchModel, cancellation);
        }

        public async Task<OperationResult> ToggleActiveStatus(long id, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var discount = await _discountRepository.GetWithTargetsAsync(id, cancellation);
            if (discount == null) return operation.Failed("رکورد یافت نشد.");

            discount.ToggleActive();
            await _discountRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        private static void AddDiscountTargets(Discount discount, DefineDiscount command)
        {
            bool hasTarget = false;

            if (command.ProductIds?.Any() == true)
            {
                foreach (var id in command.ProductIds) discount.AddTarget(DiscountTarget.ForProduct(id));
                hasTarget = true;
            }

            if (command.CategoryIds?.Any() == true)
            {
                foreach (var id in command.CategoryIds) discount.AddTarget(DiscountTarget.ForCategory(id));
                hasTarget = true;
            }

            if (command.BrandNames?.Any() == true)
            {
                foreach (var brand in command.BrandNames) discount.AddTarget(DiscountTarget.ForBrand(brand));
                hasTarget = true;
            }

            if (!hasTarget)
            {
                discount.AddTarget(DiscountTarget.ForAll());
            }
        }
    }
}