using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PollOptionConfiguration : IEntityTypeConfiguration<PollOption>
{
    public void Configure(EntityTypeBuilder<PollOption> builder)
    {
        builder.ToTable(Tables.PollOptions, Schemas.Messaging);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TextVal).IsRequired().HasMaxLength(100);

        builder.HasOne(x => x.Poll)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.PollId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
