using Microsoft.EntityFrameworkCore;
using SiteManagement.Domain.SiteSettingAgg;
using SiteManagement.Domain.TicketAgg;
using SiteManagement.Infrastructure.EFCore.Mapping;

namespace SiteManagement.Infrastructure.EFCore
{
    public class SiteContext: DbContext
    {
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }


        public SiteContext(DbContextOptions<SiteContext> options): base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var assembly = typeof(TicketMapping).Assembly;
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
