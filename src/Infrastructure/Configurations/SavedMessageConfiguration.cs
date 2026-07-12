using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SavedMessageConfiguration : IEntityTypeConfiguration<SavedMessage>
{
    public void Configure(EntityTypeBuilder<SavedMessage> builder)
    {
        builder.ToTable(Tables.SavedMessages, Schemas.Personal);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.SavedAt).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Message)
            .WithMany(x => x.SavedMessages)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
