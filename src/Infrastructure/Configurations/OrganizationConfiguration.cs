using Domain.Entities;
using Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable(Tables.Organizations, Schemas.Organizations);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ShortName).IsRequired().HasMaxLength(32);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.ShortName).IsUnique();

        builder.HasMany(x => x.Domains)
            .WithOne(x => x.Organization)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Members)
            .WithOne(x => x.Organization)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
