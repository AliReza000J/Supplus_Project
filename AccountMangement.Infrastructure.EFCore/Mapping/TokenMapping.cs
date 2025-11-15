using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AccountManagement.Domain.TokenAgg;

namespace AccountManagement.Infrastructure.EFCore.Mapping
{
    public class TokenMapping : IEntityTypeConfiguration<Token>
    {
        public void Configure(EntityTypeBuilder<Token> b)
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.AccessToken).IsRequired(false);
            b.Property(t => t.RefreshToken).IsRequired();
            b.Property(t => t.IsRevoked).HasDefaultValue(false);
            //// indexes
            //b.HasIndex(t => t.RefreshToken).IsUnique();
            //b.HasIndex(t => t.AccessToken);
        }
    }
}
