using _0_Framework.Application;
using AccountManagement.Domain.AccountAgg;
using AccountManagement.Domain.RoleAgg;
using AccountManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PharmaShared.Web.Services
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AccountContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>(); // سرویس هش پسورد شما

            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            if (!await context.Roles.AnyAsync())
            {
                context.Roles.AddRange(
                    new Role("SuperAdmin", new List<Permission>()),
                    new Role ("Admin", new List<Permission>()),
                    new Role ("Support", new List<Permission>()),
                    new Role ("ContentCreator", new List<Permission>()),
                    new Role ("User", new List<Permission>())
                );

                await context.SaveChangesAsync();
            }

            if (!await context.Accounts.AnyAsync(x => x.RoleId == 1))
            {
                var superAdmin = new Account(
                    "مدیر سیستم", "admin", "admin.png", 1);

                context.Accounts.Add(superAdmin);
                await context.SaveChangesAsync();
            }
        }
    }

}
