using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopManagement.Domain.UserProductInteractionAgg;

namespace ShopManagement.Infrastructure.EFCore.Mapping
{
    public class UserFavoriteProductMapping : IEntityTypeConfiguration<UserFavoriteProduct>
    {
        public void Configure(EntityTypeBuilder<UserFavoriteProduct> builder)
        {
            builder.ToTable("UserFavoriteProducts");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.CreationDate).IsRequired();

            builder.Property(x => x.ProductId)
                .IsRequired();

            builder.Property(x => x.UserId)
                .HasMaxLength(450)
                .IsRequired();

            builder.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
        }
    }
}
