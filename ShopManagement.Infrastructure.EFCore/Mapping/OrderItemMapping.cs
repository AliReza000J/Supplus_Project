using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopManagement.Domain.OrderAgg;


namespace ShopManagement.Infrastructure.EFCore.Mapping
{
    public class OrderItemMapping : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.ProductId).IsRequired();
            builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        }
    }
}
