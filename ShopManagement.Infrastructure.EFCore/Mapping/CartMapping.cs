using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopManagement.Domain.CartAgg;

namespace ShopManagement.Infrastructure.EFCore.Mapping
{
    public class CartMapping : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.UserId).IsRequired();
            builder.HasMany(c => c.Items).WithOne().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
