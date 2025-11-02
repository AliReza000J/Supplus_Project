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
            builder.Property(c => c.Email).HasMaxLength(200);
            builder.Property(c => c.Message).HasMaxLength(2000);
            builder.Property(c => c.AdminReply).HasMaxLength(2000);
            builder.Property(c => c.LastModified);

            //// self relation: one parent -> many children
            //builder.HasMany(c => c.Children)
            //    .WithOne(c => c.Parent)
            //    .HasForeignKey(c => c.ParentId)
            //    .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => new { c.OwnerRecordId, c.Type });
            builder.HasIndex(c => c.IsConfirmed);
            builder.HasIndex(c => c.IsCanceled);

        }
    }
}
