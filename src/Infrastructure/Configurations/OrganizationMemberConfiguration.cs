using Domain.Entities;
using Infrastructure.Extensions;
using Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ToTable(Tables.OrganizationMembers, Schemas.Organizations);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Role).IsRequired().HasCommentFromEnum();
        builder.Property(x => x.VerifiedAt).IsRequired();

        // One e-mail belongs to one domain, so a user is a member of at most one organization.
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.OrganizationId);

        builder.HasOne(x => x.User)
            .WithOne(x => x.OrganizationMembership)
            .HasForeignKey<OrganizationMember>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
