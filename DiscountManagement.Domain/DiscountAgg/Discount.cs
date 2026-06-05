using _0_Framework.Domain;
using _0_Framework.Application;
using static DiscountManagement.Domain.DiscountAgg.DiscountTarget;
using System.ComponentModel.DataAnnotations;

namespace DiscountManagement.Domain.DiscountAgg
{
    public class Discount : EntityBase
    {
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public string Title { get; private set; }
        public string? CouponCode { get; private set; }

        public DiscountType Type { get; private set; }
        public DiscountApplyType ApplyType { get; private set; }

        public int Percentage { get; private set; }

        public double? MinOrderAmount { get; private set; }

        public int? MaxDiscountQuantity { get; private set; }

        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }

        public bool IsActive { get; private set; }

        public int? MaxUsageCount { get; private set; }
        public int UsageCount { get; private set; }

        public int? UsageLimitPerUser { get; private set; }

        private readonly List<DiscountTarget> _targets = new();
        public IReadOnlyCollection<DiscountTarget> Targets => _targets.AsReadOnly();

        private readonly List<DiscountUserUsage> _usages = new();
        public IReadOnlyCollection<DiscountUserUsage> Usages => _usages.AsReadOnly();

        public bool IsProductDiscount => ApplyType == DiscountApplyType.OnProduct; 
        public bool IsOrderDiscount => ApplyType == DiscountApplyType.OnWholeOrder; 
        public bool IsAutomatic => Type == DiscountType.Automatic;
        public bool IsCoupon => Type == DiscountType.Coupon;

        public enum DiscountType
        {
            Automatic = 1,
            Coupon = 2
        }

        public enum DiscountApplyType
        {
            OnProduct = 1,
            OnWholeOrder = 2
        }

        protected Discount()
        {
            
        }

        public Discount(
            string title, string? couponCode, DiscountType type, DiscountApplyType applyType,
            int percentage, double? minOrderAmount, int? maxDiscountQuantity,
            DateTime? start, DateTime? end, int? maxUsageCount, int? usageLimitPerUser)
        {
            Title = title;
            Type = type;
            ApplyType = applyType;
            Percentage = percentage;
            CouponCode = couponCode;
            MinOrderAmount = minOrderAmount;
            MaxUsageCount = maxUsageCount;
            UsageLimitPerUser = usageLimitPerUser;
            MaxDiscountQuantity = maxDiscountQuantity;
            StartDate = start;
            EndDate = end;
            IsActive = true;
        }

        public void Edit(string title, string? couponCode, DiscountType type, DiscountApplyType applyType,
            int percentage, double? minOrderAmount, int? maxDiscountQuantity,
            DateTime? startDate, DateTime? endDate, int? maxUsageCount, int? usageLimitPerUser)
        {
            Title = title;
            CouponCode = couponCode;
            Type = type;
            ApplyType = applyType;
            Percentage = percentage;
            MinOrderAmount = minOrderAmount;
            MaxDiscountQuantity = maxDiscountQuantity;
            StartDate = startDate;
            EndDate = endDate;
            MaxUsageCount = maxUsageCount;
            UsageLimitPerUser = usageLimitPerUser;
        }

        public bool IsValid(DateTime now)
        {
            if (!IsActive)
                return false;

            if (StartDate.HasValue && now < StartDate.Value)
                return false;

            if (EndDate.HasValue && now > EndDate.Value)
                return false;

            if (MaxUsageCount.HasValue && UsageCount >= MaxUsageCount.Value)
                return false;

            return true;
        }

        public bool CanUserUse(long userId)
        {
            if (!UsageLimitPerUser.HasValue)
                return true;

            var usage = _usages.FirstOrDefault(x => x.UserId == userId);

            if (usage == null)
                return true;

            return usage.UsedCount < UsageLimitPerUser.Value;
        }

        public bool IsUsable(long userId, DateTime now)
        {
            if (!IsValid(now)) return false;
            if (!CanUserUse(userId)) return false;
            return true;
        }

        public bool AppliesToProduct(long productId, long categoryId, string brand)
        {
            if (ApplyType == DiscountApplyType.OnWholeOrder)
                return false;

            if (!_targets.Any())
                return false;

            return _targets.Any(t =>
                t.TargetType == DiscountTargetType.All ||
                (t.TargetType == DiscountTargetType.Product && t.ProductId == productId) ||
                (t.TargetType == DiscountTargetType.Brand && t.BrandName == brand) ||
                (t.TargetType == DiscountTargetType.Category && t.CategoryId == categoryId)
            );
        }

        public void ClearTargets() => _targets.Clear();

        public void AddTarget(DiscountTarget target) => _targets.Add(target);

        public void ToggleActive() => IsActive = !IsActive;

        public void IncreaseUsage(long userId)
        {
            UsageCount++;

            if (UsageLimitPerUser.HasValue)
            {
                var usage = _usages.FirstOrDefault(x => x.UserId == userId);
                if (usage == null)
                {
                    usage = new DiscountUserUsage(userId);
                    _usages.Add(usage);
                }

                usage.Increase();
            }
        }
    }
}
