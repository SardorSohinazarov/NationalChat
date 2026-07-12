using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class StickerSetConfiguration : IEntityTypeConfiguration<StickerSet>
{
    public void Configure(EntityTypeBuilder<StickerSet> builder)
    {
        builder.ToTable(Tables.StickerSets, Schemas.Storage);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(100);
    }
}
