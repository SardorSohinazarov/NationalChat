using Infrastructure.Persistence.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class BotConfiguration : IEntityTypeConfiguration<Bot>
{
    public void Configure(EntityTypeBuilder<Bot> builder)
    {
        builder.ToTable(Tables.Bots, Schemas.Bot);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Token).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Description).HasColumnType("TEXT");
        builder.Property(x => x.Commands).HasColumnType("TEXT");

        builder.HasOne(x => x.User)
            .WithMany(x => x.Bots)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
