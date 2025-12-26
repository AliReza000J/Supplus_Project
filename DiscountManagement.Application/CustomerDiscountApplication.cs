using _0_Framework.Application;
using DiscountManagement.Application.Contracts.CustomerDiscount;
using DiscountManagement.Domain.CustomerDiscountAgg;
using InventoryManagement.Domain.InventoryAgg;

namespace DiscountManagement.Application
{
    public class CustomerDiscountApplication : ICustomerDiscountApplication
    {
        private readonly ICustomerDiscountRepository _customerDiscountRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public CustomerDiscountApplication(ICustomerDiscountRepository customerDiscountRepository, IInventoryRepository inventoryRepository)
        {
            _customerDiscountRepository = customerDiscountRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<OperationResult> Define(DefineCustomerDiscount command)
        {
            var operation = new OperationResult();
            if (await _customerDiscountRepository.ExistsAsync(x => x.ProductId == command.ProductId && x.DiscountRate == command.DiscountRate
            && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var startDate = command.StartDate.ToGeorgianDateTime();
            var endDate = command.EndDate.ToGeorgianDateTime();

            var inv = await _inventoryRepository.GetBy(command.ProductId);
            if (inv == null)
                return operation.Failed("محصول در انبار تعریف نشده است!");

            int discQty = Math.Min((int)inv.CalculateCurrentCount(), command.ProductCount);

            var customerDiscount = new CustomerDiscount(command.ProductId, command.DiscountRate,
                startDate, endDate, command.Reason, discQty);
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
            && x.DiscountRate == command.DiscountRate
            && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now
            && x.Id != command.Id))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var startDate = command.StartDate.ToGeorgianDateTime();
            var endDate = command.EndDate.ToGeorgianDateTime();

            var inv = await _inventoryRepository.GetBy(command.ProductId);
            if (inv == null)
                return operation.Failed("محصول در انبار تعریف نشده است!");

            int discQty = Math.Min((int)inv.CalculateCurrentCount(), command.ProductCount);

            customerDiscount.Edit(command.ProductId, command.DiscountRate, startDate, endDate, command.Reason, discQty);
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
