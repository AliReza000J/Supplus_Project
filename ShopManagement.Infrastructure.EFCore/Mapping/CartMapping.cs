using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopManagement.Domain.CartAgg;
using ShopManagement.Domain.OrderAgg;

namespace ShopManagement.Infrastructure.EFCore.Mapping
{
    public class CartMapping : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.AccountId).IsRequired();
            builder.HasMany(c => c.Items).WithOne().OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class CartItemMapping : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.ProductId).IsRequired();
            builder.Property(i => i.Quantity).IsRequired();
            builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        }
    }
}
