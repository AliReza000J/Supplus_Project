using _0_Framework.Application;
using _0_Framework.Infrastructure;
using AccountManagement.Infrastructure.EFCore;
using InventoryManagement.Application.Contracts.Inventory;
using InventoryManagement.Domain.InventoryAgg;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.EFCore;
using System.Collections.Generic;
using System.Linq;

namespace InventoryMangement.Infrastructure.EFCore.Repository
{
    public class InventoryRepository : BaseRepository<long, Inventory>, IInventoryRepository
    {
        private readonly AccountContext _accountContext;
        private readonly ShopContext _shopContext;
        private readonly InventoryContext _inventoryContext;

        public InventoryRepository(InventoryContext inventoryContext, ShopContext shopContext,
            AccountContext accountContext) : base(inventoryContext)
        {
            _shopContext = shopContext;
            _accountContext = accountContext;
            _inventoryContext = inventoryContext;
        }

        public async Task<Inventory?> GetBy(long productId)
        {
            return await _inventoryContext.Inventory
                .FirstOrDefaultAsync(x => x.ProductId == productId);
        }

        public async Task<EditInventory> GetDetails(long id)
        {
            return await _inventoryContext.Inventory.Select(x => new EditInventory
            {
                Id = x.Id,
                ProductId = x.ProductId,
                UnitPrice = x.UnitPrice
            }).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<InventoryOperationViewModel>> GetOperationLog(long inventoryId)
        {
            var accounts = await _accountContext.Accounts.Select(x => new {x.Id, x.Fullname}).ToListAsync();
            var inventory = await _inventoryContext.Inventory
                .FirstOrDefaultAsync(x => x.Id == inventoryId);
            var operations = inventory.Operations
                .Select(x => new InventoryOperationViewModel
            {
                Id = x.Id,
                Count = x.Count,
                CurrentCount = x.CurrentCount,
                Description = x.Description,
                Operation = x.Operation,
                OperationDate = x.OperationDate.ToFarsi().ToPersianNumber(),
                OperatorId = x.OperatorId,
                OrderId = x.OrderId
            }).OrderByDescending(x => x.Id).ToList();

            foreach (var operation in operations)
            {
                operation.Operator = accounts.FirstOrDefault(x => x.Id == operation.OperatorId)?.Fullname ?? "نامشخص";
            }

            return operations;
        }

        public async Task<List<InventoryViewModel>> Search(InventorySearchModel searchModel)
        {
            var products = await _shopContext.Products
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();

            var query = _inventoryContext.Inventory.Select(x => new InventoryViewModel
            {
                Id = x.Id,
                UnitPrice = x.UnitPrice,
                InStock = x.InStock,
                ProductId = x.ProductId,
                CreationDate = x.CreationDate.ToFarsi().ToPersianNumber()
            });

            if (searchModel.ProductId > 0)
                query = query.Where(x => x.ProductId == searchModel.ProductId);

            if (searchModel.NotInStock)
                query = query.Where(x => !x.InStock);

            var inventory = await query.OrderByDescending(x => x.Id).ToListAsync();

            inventory.ForEach(item =>
                item.CurrentCount = _inventoryContext.Inventory
                    .First(i => i.Id == item.Id)
                    .CalculateCurrentCount()
            );

            inventory.ForEach(item =>
                item.Product = products.FirstOrDefault(x => x.Id == item.ProductId)?.Name
            );

            return inventory;
        }

    }
}