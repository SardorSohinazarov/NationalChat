using API.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DeviceName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.SystemVersion).IsRequired().HasMaxLength(50);
        builder.Property(x => x.AppVersion).IsRequired().HasMaxLength(50);
        builder.Property(x => x.IpAddress).IsRequired().HasMaxLength(45);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
