using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteManagement.Domain.TicketAgg;

namespace SiteManagement.Infrastructure.EFCore.Mapping
{
    public class TicketMapping : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.ToTable("Tickets");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(200).IsRequired(false);
            builder.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Message).IsRequired();
            builder.Property(x => x.TrackingCode).IsRequired();

            builder.HasIndex(x => x.TrackingCode)
            .IsUnique();
        }
    }
}
