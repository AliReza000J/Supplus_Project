using CommentManagement.Domain.CommentAgg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommnetManagement.Infrastructure.EFCore.Mapping
{
    public class CommentReactionMapping : IEntityTypeConfiguration<CommentReaction>
    {
        public void Configure(EntityTypeBuilder<CommentReaction> builder)
        {
            builder.ToTable("CommentReactions");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.UserIdentifier).IsRequired().HasMaxLength(200);
            builder.HasIndex(r => new { r.CommentId, r.UserIdentifier }).IsUnique();

        }

    }
}
