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
        builder.Property(x => x.Domain).IsRequired().HasMaxLength(253);
        builder.Property(x => x.ShortName).IsRequired().HasMaxLength(32);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.Domain).IsUnique();

        builder.HasMany(x => x.Members)
            .WithOne(x => x.Organization)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
