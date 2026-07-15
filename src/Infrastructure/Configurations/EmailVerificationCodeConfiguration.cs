using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class EmailVerificationCodeConfiguration : IEntityTypeConfiguration<EmailVerificationCode>
{
    public void Configure(EntityTypeBuilder<EmailVerificationCode> builder)
    {
        builder.ToTable(Tables.EmailVerificationCodes, Schemas.Security);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(254);
        builder.Property(x => x.CodeHash).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Purpose).IsRequired();
        builder.Property(x => x.AttemptCount).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.RequestIpAddress).HasMaxLength(45);

        builder.HasIndex(x => new { x.Email, x.Purpose, x.ExpiresAt });
    }
}
