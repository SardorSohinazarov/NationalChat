using Infrastructure.Persistence.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SecretMessageConfiguration : IEntityTypeConfiguration<SecretMessage>
{
    public void Configure(EntityTypeBuilder<SecretMessage> builder)
    {
        builder.ToTable(Tables.SecretMessages, Schemas.Messaging);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Ciphertext).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // A sequence number is accepted once per sending device; this also settles concurrent retries.
        builder.HasIndex(x => new { x.SecretChatId, x.SenderSessionId, x.Seq }).IsUnique();
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.SecretChat)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.SecretChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
