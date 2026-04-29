using DiscountManagement.Domain.DiscountAgg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscountManagement.Infrastructure.EFCore.Mapping
{
    public class DiscountMapping : IEntityTypeConfiguration<Discount>
    {
        public void Configure(EntityTypeBuilder<Discount> builder)
        {
            builder.ToTable("Discounts");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).HasMaxLength(255).IsRequired();
            builder.Property(x => x.CouponCode).HasMaxLength(50);

            builder.Property(x => x.Percentage).IsRequired();
            builder.Property(x => x.MinOrderAmount).HasColumnType("decimal(18,2)");

            builder.HasMany(x => x.Targets)
                .WithOne()
                .HasForeignKey("DiscountId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Usages)
                .WithOne()
                .HasForeignKey("DiscountId")
                .OnDelete(DeleteBehavior.Cascade);
        }

        public class DiscountTargetMapping : IEntityTypeConfiguration<DiscountTarget>
        {
            public void Configure(EntityTypeBuilder<DiscountTarget> builder)
            {
                builder.ToTable("DiscountTargets");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.BrandName).HasMaxLength(150);

                builder.HasIndex(x => x.ProductId);
                builder.HasIndex(x => x.CategoryId);
            }
        }

        public class DiscountUserUsageMapping : IEntityTypeConfiguration<DiscountUserUsage>
        {
            public void Configure(EntityTypeBuilder<DiscountUserUsage> builder)
            {
                builder.ToTable("DiscountUserUsages");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.UsedCount);

                builder.HasIndex(x => x.UserId);
            }
        }
    }
}