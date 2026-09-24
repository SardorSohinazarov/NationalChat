using Domain.Entities;
using Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class OrganizationDomainConfiguration : IEntityTypeConfiguration<OrganizationDomain>
{
    public void Configure(EntityTypeBuilder<OrganizationDomain> builder)
    {
        builder.ToTable(Tables.OrganizationDomains, Schemas.Organizations);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Domain).IsRequired().HasMaxLength(253);
        builder.HasIndex(x => x.Domain).IsUnique();
    }
}
