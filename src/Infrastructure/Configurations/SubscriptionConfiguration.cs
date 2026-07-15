using Infrastructure.Persistence.Constants;
using Domain.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable(Tables.Subscriptions, Schemas.Personal);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasCommentFromEnum();
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.PricePaid).HasColumnType("decimal(10,2)");

        builder.HasOne(x => x.User)
            .WithMany(x => x.Subscriptions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
