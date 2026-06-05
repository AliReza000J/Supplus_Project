using AccountManagement.Domain.ShippingInfoAgg;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.Infrastructure.EFCore.Mapping
{
    public class ShippingInfoMapping : IEntityTypeConfiguration<ShippingInfo>
    {
        public void Configure(EntityTypeBuilder<ShippingInfo> builder)
        {
            builder.ToTable("ShippingInfos");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReceiverFullName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.ReceiverMobile).HasMaxLength(20).IsRequired();
            builder.Property(x => x.ReceiverAddress).HasMaxLength(1500).IsRequired();
            builder.Property(x => x.ReceiverZipCode).HasMaxLength(20).IsRequired();
        }
    }
}
