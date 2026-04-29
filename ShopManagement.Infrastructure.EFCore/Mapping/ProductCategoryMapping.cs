using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopManagement.Domain.ProductCategoryAgg;

namespace ShopManagement.Infrastructure.EFCore.Mapping
{
    public class ProductCategoryMapping : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.ToTable("ProductCategories");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
            builder.Property(x => x.Description).IsRequired(false);
            builder.Property(x => x.Picture).HasMaxLength(1024);
            builder.Property(x => x.PictureAlt).HasMaxLength(255);
            builder.Property(x => x.PictureTitle).HasMaxLength(500);
            builder.Property(x => x.Keywords).HasMaxLength(80).IsRequired();
            builder.Property(x => x.MetaDescription).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Slug).HasMaxLength(250).IsRequired();
            builder.Property(x => x.FullSlug).HasMaxLength(1000).IsRequired(false);

            // Self-referencing relationship
            builder.HasOne(c => c.Parent)
                   .WithMany(c => c.Children)
                   .HasForeignKey(c => c.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Products)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.ParentId);
            builder.HasIndex(c => c.Slug).IsUnique(false);
            builder.HasIndex(x => x.FullSlug).IsUnique(false);
        }
    }
}
