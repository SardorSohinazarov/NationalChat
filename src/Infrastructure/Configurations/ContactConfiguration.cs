using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable(Tables.Contacts, Schemas.Identity);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.CustomFirstName).HasMaxLength(100);
        builder.Property(x => x.CustomLastName).HasMaxLength(100);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ContactUser)
            .WithMany(x => x.ContactedBy)
            .HasForeignKey(x => x.ContactUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
