using Infrastructure.Persistence.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class FolderChatConfiguration : IEntityTypeConfiguration<FolderChat>
{
    public void Configure(EntityTypeBuilder<FolderChat> builder)
    {
        builder.ToTable(Tables.FolderChats, Schemas.Personal);

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Folder)
            .WithMany(x => x.FolderChats)
            .HasForeignKey(x => x.FolderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Chat)
            .WithMany()
            .HasForeignKey(x => x.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
