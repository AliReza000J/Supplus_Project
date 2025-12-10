using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopManagement.Domain.CartAgg;

namespace ShopManagement.Infrastructure.EFCore.Mapping
{
    public class CartItemMapping : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.ProductId).IsRequired();
            builder.Property(i => i.Quantity).IsRequired();
            builder.Property(i => i.CartKey).IsRequired();
        }
    }
}
