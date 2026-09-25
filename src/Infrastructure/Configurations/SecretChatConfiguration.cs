using Infrastructure.Extensions;
using Infrastructure.Persistence.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SecretChatConfiguration : IEntityTypeConfiguration<SecretChat>
{
    public void Configure(EntityTypeBuilder<SecretChat> builder)
    {
        builder.ToTable(Tables.SecretChats, Schemas.Chat);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.InitiatorPublicKey).IsRequired().HasMaxLength(64);
        builder.Property(x => x.ParticipantPublicKey).HasMaxLength(64);
        builder.Property(x => x.Status).IsRequired().HasCommentFromEnum();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.InitiatorSessionId, x.Status });
        builder.HasIndex(x => new { x.ParticipantSessionId, x.Status });
        builder.HasIndex(x => new { x.ParticipantId, x.Status });

        builder.HasOne(x => x.Initiator)
            .WithMany()
            .HasForeignKey(x => x.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Participant)
            .WithMany()
            .HasForeignKey(x => x.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InitiatorSession)
            .WithMany()
            .HasForeignKey(x => x.InitiatorSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ParticipantSession)
            .WithMany()
            .HasForeignKey(x => x.ParticipantSessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
