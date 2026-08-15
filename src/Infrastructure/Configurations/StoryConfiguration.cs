using Infrastructure.Persistence.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.ToTable(Tables.Stories, Schemas.Story);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Caption).HasMaxLength(255);
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Stories)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
