using CommentManagement.Domain.CommentAgg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommnetManagement.Infrastructure.EFCore.Mapping
{
    public class CommentMapping : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Username).IsRequired().HasMaxLength(200);
            builder.Property(c => c.UserIdentifier).IsRequired();
            builder.Property(c => c.Message).HasMaxLength(4000);
            builder.Property(c => c.AdminReply).HasMaxLength(4000);
            builder.Property(c => c.LastModified);

            builder.HasIndex(c => new { c.OwnerRecordId, c.Type });
            builder.HasIndex(c => c.IsConfirmed);
            builder.HasIndex(c => c.IsCanceled);

        }
    }
}
