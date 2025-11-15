using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopManagement.Domain.OrderAgg;


namespace ShopManagement.Infrastructure.EFCore.Mapping
{
    public class OrderMapping : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.AccountId).IsRequired();
            builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            builder.HasMany(o => o.Items).WithOne(o=>o.Order)
                .HasForeignKey(o=>o.OrderId).OnDelete(DeleteBehavior.Cascade);
        }
    }

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
