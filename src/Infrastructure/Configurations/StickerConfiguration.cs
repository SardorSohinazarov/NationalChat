using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class StickerConfiguration : IEntityTypeConfiguration<Sticker>
{
    public void Configure(EntityTypeBuilder<Sticker> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Emoji).IsRequired().HasMaxLength(10);

        builder.HasOne(x => x.StickerSet)
            .WithMany(x => x.Stickers)
            .HasForeignKey(x => x.SetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
