using Infrastructure.Persistence.Constants;
using Domain.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.ToTable(Tables.Chats, Schemas.Chat);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasCommentFromEnum();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.DeletedAt);
        builder.HasQueryFilter(x => x.DeletedAt == null);

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Chat)
            .HasForeignKey(x => x.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Members)
            .WithOne(x => x.Chat)
            .HasForeignKey(x => x.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
