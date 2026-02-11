using BlogManagement.Domain.UserArticleInteractionAgg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogManagement.Infrastructure.EFCore.Mapping
{
    public class ArticleViewMapping : IEntityTypeConfiguration<ArticleView>
    {
        public void Configure(EntityTypeBuilder<ArticleView> builder)
        {
            builder.ToTable("ArticleViews");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.CreationDate).IsRequired();

            builder.Property(x => x.ArticleId)
                .IsRequired();

            builder.Property(x => x.UserId)
                .HasDefaultValue(0);
        }
    }
}
