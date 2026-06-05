using _0_Framework.Domain;
using DiscountManagement.Application.Contracts.Discount;

namespace DiscountManagement.Domain.DiscountAgg
{
    public interface IDiscountRepository : IRepository<long, Discount>
    {
        Task<EditDiscount?> GetDetails(long id, CancellationToken cancellation);
        Task<List<DiscountViewModel>> Search(DiscountSearchModel searchModel, CancellationToken cancellation);
        Task<List<Discount>> GetByIds(List<long> discountIds, CancellationToken cancellation);

        Task<List<Discount>> GetActiveAutomaticDiscounts(List<long> productIds,
        List<long> categoryIds, List<string> brands, CancellationToken cancellation);
        Task<Discount?> GetByCouponCode(string couponCode, CancellationToken cancellation);
    }
}
