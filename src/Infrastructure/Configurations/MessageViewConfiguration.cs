using API.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class MessageViewConfiguration : IEntityTypeConfiguration<MessageView>
{
    public void Configure(EntityTypeBuilder<MessageView> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ViewedAt).IsRequired();

        builder.HasOne(x => x.Message)
            .WithMany(x => x.Views)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
