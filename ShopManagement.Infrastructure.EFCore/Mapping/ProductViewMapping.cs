using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopManagement.Domain.UserProductInteractionAgg;

namespace ShopManagement.Infrastructure.EFCore.Mapping
{
    public class ProductViewMapping : IEntityTypeConfiguration<ProductView>
    {
        public void Configure(EntityTypeBuilder<ProductView> builder)
        {
            builder.ToTable("ProductViews");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.CreationDate).IsRequired();

            builder.Property(x => x.ProductId)
                .IsRequired();

            builder.Property(x => x.UserId)
                .HasDefaultValue(0);
        }
    }
}
