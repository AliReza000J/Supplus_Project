using _0_Framework.Application;
using _0_Framework.Infrastructure;
using DiscountManagement.Application.Contracts.Discount;
using DiscountManagement.Domain.DiscountAgg;
using Microsoft.EntityFrameworkCore;

namespace DiscountManagement.Infrastructure.EFCore.Repository
{
    public class DiscountRepository
        : RepositoryBase<long, Discount>, IDiscountRepository
    {
        private readonly DiscountContext _context;

        public DiscountRepository(DiscountContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Discount>> GetActiveAutomaticDiscounts(List<long> productIds,
            List<long> categoryIds, List<string> brands, CancellationToken cancellation)
        {
            var now = DateTime.Now;

            return await _context.Discounts
                .Include(x => x.Targets)
                .Where(x => x.IsActive && x.Type == Discount.DiscountType.Automatic)
                .Where(x => (x.StartDate == null || x.StartDate <= now) && (x.EndDate == null || x.EndDate >= now))
                .Where(x => x.Targets.Any(t =>
                    t.TargetType == DiscountTarget.DiscountTargetType.All ||
                    (t.TargetType == DiscountTarget.DiscountTargetType.Product && productIds.Contains(t.ProductId ?? 0)) ||
                    (t.TargetType == DiscountTarget.DiscountTargetType.Category && categoryIds.Contains(t.CategoryId ?? 0)) ||
                    (t.TargetType == DiscountTarget.DiscountTargetType.Brand && brands.Contains(t.BrandName ?? ""))
                ))
                .ToListAsync(cancellation);
        }

        public async Task<List<Discount>> GetByIds(List<long> discountIds, CancellationToken cancellation)
        {
            return await _context.Discounts
                .Where(x => discountIds.Contains(x.Id))
                .ToListAsync(cancellation);
        }

        public async Task<Discount?> GetByCouponCode(string couponCode, CancellationToken cancellation)
        {
            return await _context.Discounts
                .Include(x => x.Targets)
                .Include(x => x.Usages)
                .FirstOrDefaultAsync(x => x.CouponCode == couponCode, cancellation);
        }

        public async Task<List<DiscountViewModel>> Search(DiscountSearchModel searchModel, CancellationToken cancellation)
        {
            var query = _context.Discounts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchModel.Title))
                query = query.Where(x => x.Title.Contains(searchModel.Title));

            if (!string.IsNullOrWhiteSpace(searchModel.CouponCode))
                query = query.Where(x => !string.IsNullOrWhiteSpace(x.CouponCode) && x.CouponCode.Contains(searchModel.CouponCode.ToUpper()));

            if (searchModel.IsActive.HasValue && searchModel.IsActive.Value)
                query = query.Where(x => x.IsActive);

            if (searchModel.Type.HasValue)
                query = query.Where(x => (int)x.Type == searchModel.Type);

            if (searchModel.ApplyType.HasValue)
                query = query.Where(x => (int)x.ApplyType == searchModel.ApplyType);

            if (!string.IsNullOrWhiteSpace(searchModel.StartDate))
            {
                var start = searchModel.StartDate.ToGregorianDateTime();
                query = query.Where(x => x.StartDate > start);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.EndDate))
            {
                var end = searchModel.EndDate.ToGregorianDateTime();
                query = query.Where(x => x.EndDate < end);
            }

            var discounts = await query
                .OrderByDescending(x => x.Id)
                .Select(x => new DiscountViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    CouponCode = x.CouponCode ?? "---",
                    Percentage = x.Percentage,
                    IsActive = x.IsActive,
                    UsageCount = x.UsageCount,
                    TypeName = x.Type == Discount.DiscountType.Automatic ? "خودکار" : "کد تخفیف",
                    ApplyTypeName = x.ApplyType == Discount.DiscountApplyType.OnProduct ? "روی محصول" : "کل سفارش",
                    StartDate = x.StartDate.ToFarsi(),
                    EndDate = x.EndDate.ToFarsi(),
                    CreationDate = x.CreationDate.ToFarsi()
                })
                .ToListAsync(cancellation);

            return discounts;
        }

        public async Task<EditDiscount?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _context.Discounts
                .Include(x => x.Targets)
                .Select(x => new EditDiscount
                {
                    Id = x.Id,
                    Title = x.Title,
                    CouponCode = x.CouponCode,
                    Percentage = x.Percentage,
                    Type = (int)x.Type,
                    ApplyType = (int)x.ApplyType,
                    MinOrderAmount = x.MinOrderAmount,
                    MaxDiscountQuantity = x.MaxDiscountQuantity,
                    MaxUsageCount = x.MaxUsageCount,
                    UsageLimitPerUser = x.UsageLimitPerUser,
                    StartDate = x.StartDate.ToFarsi(),
                    EndDate = x.EndDate.ToFarsi(),
                    ProductIds = x.Targets.Where(t => t.TargetType == DiscountTarget.DiscountTargetType.Product).Select(t => t.ProductId ?? 0).ToList(),
                    CategoryIds = x.Targets.Where(t => t.TargetType == DiscountTarget.DiscountTargetType.Category).Select(t => t.CategoryId ?? 0).ToList(),
                    BrandNames = x.Targets.Where(t => t.TargetType == DiscountTarget.DiscountTargetType.Brand).Select(t => t.BrandName ?? "").ToList()
                })
                .FirstOrDefaultAsync(x => x.Id == id, cancellation);
        }
    }
}
