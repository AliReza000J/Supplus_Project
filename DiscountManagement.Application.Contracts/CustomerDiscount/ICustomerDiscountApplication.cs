using _0_Framework.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountManagement.Application.Contracts.CustomerDiscount
{
    public interface ICustomerDiscountApplication
    {
        Task<OperationResult> Define(DefineCustomerDiscount command);
        Task<OperationResult> Edit(EditCustoemrDiscount command);
        Task<EditCustoemrDiscount> GetDetails(long id);
        Task<List<CustomerDiscountViewModel>> Search(CustomerDiscountSearchModel searchModel);
    }
}
