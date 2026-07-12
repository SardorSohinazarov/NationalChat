using API.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TwoFactorAuthConfiguration : IEntityTypeConfiguration<TwoFactorAuth>
{
    public void Configure(EntityTypeBuilder<TwoFactorAuth> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Hint).HasMaxLength(255);
        builder.Property(x => x.RecoveryEmail).HasMaxLength(255);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
