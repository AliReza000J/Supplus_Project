using AccountManagement.Application;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Application.Contracts.Role;
using AccountManagement.Domain.AccountAgg;
using AccountManagement.Domain.RoleAgg;
using AccountManagement.Domain.TokenAgg;
using AccountManagement.Infrastructure.EFCore.Repository;
using AccountManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using _0_Framework.Infrastructure;
using AccountManagement.Configuration.Permissions;
using AccountManagement.Domain.ShippingInfoAgg;
using AccountManagement.Application.Contracts.ShippingInfo;
using _01_PharmaPlusQuery.Query;
using _01_PharmaPlusQuery.Contracts.Account;


namespace AccountManagement.Configuration
{
    public class AccountManagementBootstrapper
    {
        public static void Configure(IServiceCollection services, string connectionString)
        {
            services.AddTransient<IAccountApplication, AccountApplication>();
            services.AddTransient<IAccountRepository, AccountRepository>();

            services.AddTransient<IRoleApplication, RoleApplication>();
            services.AddTransient<IRoleRepository, RoleRepository>();

            services.AddTransient<ITokenRepository, TokenRepository>();

            services.AddScoped<IShippingInfoRepository, ShippingInfoRepository>();
            services.AddScoped<IShippingInfoApplication, ShippingInfoApplication>();

            services.AddTransient<IAccountQuery, AccountQuery>();

            services.AddTransient<IPermissionExposer, AccountPermissionExposer>();
            services.AddDbContext<AccountContext>(x => x.UseSqlServer(connectionString));
        }
    }
}
