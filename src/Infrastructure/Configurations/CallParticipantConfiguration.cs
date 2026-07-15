using Infrastructure.Persistence.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class CallParticipantConfiguration : IEntityTypeConfiguration<CallParticipant>
{
    public void Configure(EntityTypeBuilder<CallParticipant> builder)
    {
        builder.ToTable(Tables.CallParticipants, Schemas.Call);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.JoinedAt).IsRequired();

        builder.HasOne(x => x.Call)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.CallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
