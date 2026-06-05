using _0_Framework.Application;
using _0_Framework.Infrastructure;
using AccountManagement.Infrastructure.EFCore;
using InventoryManagement.Application.Contracts.Inventory;
using InventoryManagement.Domain.InventoryAgg;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.ProductAgg;
using ShopManagement.Infrastructure.EFCore;
using System.Collections.Generic;
using System.Linq;

namespace InventoryMangement.Infrastructure.EFCore.Repository
{
    public class InventoryRepository : RepositoryBase<long, Inventory>, IInventoryRepository
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

        public async Task<Inventory?> GetBy(long productId, CancellationToken cancellation)
        {
            return await _inventoryContext.Inventory.Include(x => x.Operations)
                .FirstOrDefaultAsync(x => x.ProductId == productId, cancellation);
        }

        public async Task<List<Inventory>> GetByProductIds(List<long> productIds, CancellationToken cancellation)
        {
            return await _inventoryContext.Inventory
                .Where(x => productIds.Contains(x.ProductId))
                .ToListAsync(cancellation);
        }

        public async Task<EditInventory?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _inventoryContext.Inventory.Select(x => new EditInventory
            {
                Id = x.Id,
                ProductId = x.ProductId,
                UnitPrice = x.UnitPrice
            }).FirstOrDefaultAsync(x => x.Id == id, cancellation);
        }

        public async Task<List<InventoryOperationViewModel>> GetOperationLog(long inventoryId, CancellationToken cancellation)
        {
            var accounts = await _accountContext.Accounts.Select(x => new { x.Id, x.Fullname }).ToListAsync(cancellation);
            var inventory = await _inventoryContext.Inventory
                .FirstOrDefaultAsync(x => x.Id == inventoryId, cancellation);

            var operations = inventory.Operations
                .Select(x => new InventoryOperationViewModel
                {
                    Id = x.Id,
                    Count = x.Count,
                    CurrentCount = x.CurrentCount,
                    Description = x.Description,
                    Operation = x.Operation,
                    OperationDate = x.OperationDate.ToFarsi(),
                    OperatorId = x.OperatorId,
                    OrderId = x.OrderId
                }).OrderByDescending(x => x.Id).ToList();

            foreach (var operation in operations)
            {
                operation.OperatorName = accounts.FirstOrDefault(x => x.Id == operation.OperatorId)?.Fullname ?? "نامشخص";
            }

            return operations;
        }

        public async Task<List<Inventory>> GetInvListByIdsAsync(List<long> productIds, CancellationToken cancellation)
        {
            return await _inventoryContext.Inventory
                    .Where(x => productIds.Contains(x.ProductId))
                    .ToListAsync(cancellation);
        }

        public async Task<List<InventoryViewModel>> Search(InventorySearchModel searchModel, CancellationToken cancellation)
        {
            var products = await _shopContext.Products
                .Select(x => new { x.Id, x.Name })
                .ToListAsync(cancellation);

            var query = _inventoryContext.Inventory.Select(x => new InventoryViewModel
            {
                Id = x.Id,
                UnitPrice = x.UnitPrice,
                InStock = x.InStock,
                ProductId = x.ProductId,
                CreationDate = x.CreationDate.ToFarsi()
            });

            if (searchModel.ProductId > 0)
                query = query.Where(x => x.ProductId == searchModel.ProductId);

            if (searchModel.NotInStock)
                query = query.Where(x => !x.InStock);

            var inventory = await query.OrderByDescending(x => x.Id).ToListAsync(cancellation);

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