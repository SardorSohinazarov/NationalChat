using API.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SecretChatConfiguration : IEntityTypeConfiguration<SecretChat>
{
    public void Configure(EntityTypeBuilder<SecretChat> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EncryptionKey).IsRequired().HasColumnType("TEXT");
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Initiator)
            .WithMany()
            .HasForeignKey(x => x.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Participant)
            .WithMany()
            .HasForeignKey(x => x.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
