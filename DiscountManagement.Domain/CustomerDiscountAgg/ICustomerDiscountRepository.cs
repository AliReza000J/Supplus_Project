using _0_Framework.Domain;
using DiscountManagement.Application.Contracts.CustomerDiscount;

namespace DiscountManagement.Domain.CustomerDiscountAgg
{
    public interface ICustomerDiscountRepository: IRepository<long, CustomerDiscount>
    {
        Task<CustomerDiscount?> GetByProductId(long productId, CancellationToken cancellation);
        Task<EditCustoemrDiscount?> GetDetails(long id, CancellationToken cancellation);
        Task<List<CustomerDiscountViewModel>> Search(CustomerDiscountSearchModel searchModel, CancellationToken cancellation);
    }
}
