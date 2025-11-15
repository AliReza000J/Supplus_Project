using _0_Framework.Infrastructure;
using InventoryManagement.Application;
using InventoryManagement.Application.Contracts.Inventory;
using InventoryManagement.Domain.InventoryAgg;
using InventoryManagement.Infrastructure.Configuration.Permissions;
using InventoryMangement.Infrastructure.EFCore;
using InventoryMangement.Infrastructure.EFCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.Configuration
{
    public class InventoryManagementBootstrapper
    {
        public static void Configure(IServiceCollection services, string connectionString)
        {
            services.AddTransient<IInventoryRepository, InventoryRepository>();
            services.AddTransient<IInventoryApplication, InventoryApplication>();

            services.AddTransient<IPermissionExposer, InventoryPermissionExposer>();

            services.AddDbContext<InventoryContext>(x=>x.UseSqlServer(connectionString));
        }
    }
}
