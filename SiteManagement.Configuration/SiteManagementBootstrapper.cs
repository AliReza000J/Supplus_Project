using _01_PharmaPlusQuery.Contracts.Ticket;
using _01_PharmaPlusQuery.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteManagement.Application;
using SiteManagement.Application.Contracts.Ticket;
using SiteManagement.Domain.SiteSettingAgg;
using SiteManagement.Domain.TicketAgg;
using SiteManagement.Infrastructure.EFCore;
using SiteManagement.Infrastructure.EFCore.Repository;

namespace SiteManagement.Configuration
{
    public class SiteManagementBootstrapper
    {
        public static void Configure(IServiceCollection services, string connectionString)
        {
            services.AddTransient<ITicketRepository, TicketRepository>();
            services.AddTransient<ITicketApplication, TicketApplication>();

            services.AddScoped<ISiteSettingRepository, SiteSettingRepository>();
            
            services.AddScoped<UpdateSiteSettingCommand>();
            services.AddScoped<GetSiteSettingQuery>();
            services.AddScoped<GetLogoFromSetting>();
            services.AddTransient<ITicketQuery, TicketQuery>();

            services.AddDbContext<SiteContext>(x => x.UseSqlServer(connectionString));
        }
    }
}