using Infrastructure.Persistence.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SecretFileConfiguration : IEntityTypeConfiguration<SecretFile>
{
    public void Configure(EntityTypeBuilder<SecretFile> builder)
    {
        builder.ToTable(Tables.SecretFiles, Schemas.Messaging);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.SecretChatId);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.SecretChat)
            .WithMany(x => x.Files)
            .HasForeignKey(x => x.SecretChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
