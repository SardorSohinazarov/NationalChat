using Infrastructure.Persistence.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable(Tables.Groups, Schemas.Chat);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Description).HasColumnType("TEXT");
        builder.Property(x => x.InviteLink).HasMaxLength(255);
        builder.Property(x => x.AutoJoin).IsRequired();
        builder.HasIndex(x => x.ChatId).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.AutoJoin });

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Photo)
            .WithMany()
            .HasForeignKey(x => x.PhotoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Chat)
            .WithMany(x => x.Groups)
            .HasForeignKey(x => x.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Creator)
            .WithMany(x => x.CreatedGroups)
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
