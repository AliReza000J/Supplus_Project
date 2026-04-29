using _0_Framework.Application;

namespace DiscountManagement.Application.Contracts.Discount
{
    public interface IDiscountApplication
    {
        Task<OperationResult> Define(DefineDiscount command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditDiscount command, CancellationToken cancellation);
        Task<EditDiscount?> GetDetails(long id, CancellationToken cancellation);
        Task<List<DiscountViewModel>> Search(DiscountSearchModel searchModel, CancellationToken cancellation);
        Task<OperationResult> ToggleActiveStatus(long id, CancellationToken cancellation);
        Task<DiscountCalculationResult> CalculateDiscountsAsync(DiscountCalculationModel command, CancellationToken cancellation);

        Task IncreaseUsage(long discountId, long userId, CancellationToken cancellation);
    }

    public class DiscountViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string CouponCode { get; set; }
        public string TypeName { get; set; }
        public string ApplyTypeName { get; set; }
        public int Percentage { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool IsActive { get; set; }
        public int UsageCount { get; set; } // تعداد دفعات استفاده شده
        public string CreationDate { get; set; }
    }

    public class DiscountSearchModel
    {
        public string? Title { get; set; }
        public string? CouponCode { get; set; }
        public int? Type { get; set; } // 1: Automatic, 2: Coupon
        public int? ApplyType { get; set; } // 1: OnProduct, 2: OnOrder
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
