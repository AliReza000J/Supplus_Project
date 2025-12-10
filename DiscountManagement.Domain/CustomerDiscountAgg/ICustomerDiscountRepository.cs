using _0_Framework.Domain;
using DiscountManagement.Application.Contracts.CustomerDiscount;

namespace DiscountManagement.Domain.CustomerDiscountAgg
{
    public interface ICustomerDiscountRepository: IRepository<long, CustomerDiscount>
    {
        Task<EditCustoemrDiscount> GetDetails(long id);
        Task<List<CustomerDiscountViewModel>> Search(CustomerDiscountSearchModel searchModel);
    }
}
