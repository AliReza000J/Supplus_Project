using _0_Framework.Application;
using DiscountManagement.Application.Contracts.CustomerDiscount;
using DiscountManagement.Domain.CustomerDiscountAgg;

namespace DiscountManagement.Application
{
    public class CustomerDiscountApplication : ICustomerDiscountApplication
    {
        private readonly ICustomerDiscountRepository _customerDiscountRepository;

        public CustomerDiscountApplication(ICustomerDiscountRepository customerDiscountRepository)
        {
            _customerDiscountRepository = customerDiscountRepository;
        }

        public async Task<OperationResult> Define(DefineCustomerDiscount command)
        {
            var operation = new OperationResult();
            if (await _customerDiscountRepository.ExistsAsync(x => x.ProductId == command.ProductId && x.DiscountRate == command.DiscountRate))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var startDate = command.StartDate.ToGeorgianDateTime();
            var endDate = command.EndDate.ToGeorgianDateTime();
            var customerDiscount = new CustomerDiscount(command.ProductId, command.DiscountRate,
                startDate, endDate, command.Reason, command.ProductCount);
            _customerDiscountRepository.Create(customerDiscount);
            await _customerDiscountRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditCustoemrDiscount command)
        {
            var operation = new OperationResult();
            var customerDiscount = await _customerDiscountRepository.GetAsync(command.Id);

            if (customerDiscount == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _customerDiscountRepository.ExistsAsync(x => x.ProductId == command.ProductId
            && x.DiscountRate == command.DiscountRate && x.Id != command.Id))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var startDate = command.StartDate.ToGeorgianDateTime();
            var endDate = command.EndDate.ToGeorgianDateTime();
            customerDiscount.Edit(command.ProductId, command.DiscountRate, startDate, endDate, command.Reason, command.ProductCount);
            await _customerDiscountRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<EditCustoemrDiscount> GetDetails(long id)
        {
            return await _customerDiscountRepository.GetDetails(id);
        }

        public async Task<List<CustomerDiscountViewModel>> Search(CustomerDiscountSearchModel searchModel)
        {
            return await _customerDiscountRepository.Search(searchModel);
        }
    }
}
