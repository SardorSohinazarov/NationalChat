using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ChannelSubscriberConfiguration : IEntityTypeConfiguration<ChannelSubscriber>
{
    public void Configure(EntityTypeBuilder<ChannelSubscriber> builder)
    {
        builder.ToTable(Tables.ChannelSubscribers, Schemas.Chat);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.JoinedAt).IsRequired();

        builder.HasOne(x => x.Channel)
            .WithMany(x => x.Subscribers)
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
