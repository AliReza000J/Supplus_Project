using _0_Framework.Application;
using InventoryManagement.Application.Contracts.Inventory;
using InventoryManagement.Domain.InventoryAgg;

namespace InventoryManagement.Application
{
    public class InventoryApplication : IInventoryApplication
    {
        private readonly IAuthHelper _authHelper;
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryApplication(IInventoryRepository inventoryRepository, IAuthHelper authHelper)
        {
            _inventoryRepository = inventoryRepository;
            _authHelper = authHelper;
        }

        public async Task<OperationResult> Create(CreateInventory command)
        {
            var operation = new OperationResult();
            if (await _inventoryRepository
                .ExistsAsync(x => x.ProductId == command.ProductId))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var inventory = new Inventory(command.ProductId, command.UnitPrice);
            _inventoryRepository.Create(inventory);
            await _inventoryRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditInventory command)
        {
            var operation = new OperationResult();
            var inventory = await _inventoryRepository.GetAsync(command.Id);
            if (inventory == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _inventoryRepository.ExistsAsync(x => x.ProductId == command.ProductId && x.Id != command.Id))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            inventory.Edit(command.ProductId, command.UnitPrice);
            await _inventoryRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<EditInventory> GetDetails(long id)
        {
            return await _inventoryRepository.GetDetails(id);
        }

        public async Task<List<InventoryOperationViewModel>> GetOperationLog(long inventoryId)
        {
            return await _inventoryRepository.GetOperationLog(inventoryId);
        }

        public async Task<OperationResult> Increase(IncreaseInventory command)
        {
            var operation = new OperationResult();
            var inventory = await _inventoryRepository.GetAsync(command.InventoryId);
            if (inventory == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (command.Count <= 0)
                return operation.Failed("مقدار برای فیلد تعداد افزایش مجاز نیست.");

            var operatorId = _authHelper.CurrentAccountId();
            inventory.Increase(command.Count, operatorId, command.Description);
            await _inventoryRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<OperationResult> ReduceByAdmin(ReduceInventory command)
        {
            var operation = new OperationResult();
            var inventory = await _inventoryRepository.GetAsync(command.InventoryId);
            if (inventory == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (command.Count <= 0)
                return operation.Failed("مقدار برای فیلد تعداد کاهش مجاز نیست.");

            if (inventory.CalculateCurrentCount() < command.Count)
                return operation.Failed("تعداد کاهش از تعداد موجودی بیشتر است!");

            var operatorId = _authHelper.CurrentAccountId();
            inventory.Reduce(command.Count, operatorId, command.Description, command.OrderId);
            await _inventoryRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<OperationResult> ReduceByOrder(ReduceInventoryByOrder command)
        {
            var operation = new OperationResult();
            var inventory = await _inventoryRepository.GetAsync(command.InventoryId);
            if (inventory == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            inventory.Reduce(command.Count, command.AccountId, command.Description, command.OrderId);
            await _inventoryRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<List<InventoryViewModel>> Search(InventorySearchModel searchModel)
        {
            return await _inventoryRepository.Search(searchModel);
        }
    }
}