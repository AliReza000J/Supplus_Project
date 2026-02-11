using _0_Framework.Application;

namespace DiscountManagement.Application.Contracts.CustomerDiscount
{
    public interface ICustomerDiscountApplication
    {
        Task<OperationResult> Define(DefineCustomerDiscount command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditCustoemrDiscount command, CancellationToken cancellation);
        Task<EditCustoemrDiscount?> GetDetails(long id, CancellationToken cancellation);
        Task<List<CustomerDiscountViewModel>> Search(CustomerDiscountSearchModel searchModel, CancellationToken cancellation);
    }
}
