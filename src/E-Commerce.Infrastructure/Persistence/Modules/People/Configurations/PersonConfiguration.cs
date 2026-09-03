using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Enums;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.People.Configurations;

public sealed class PersonConfiguration : BaseEntityConfiguration<Person>
{
    public override void Configure(EntityTypeBuilder<Person> builder)
    {
        base.Configure(builder); // audit, soft delete, query filter

        // Table
        builder.ToTable("People", "user");

        // Primary key
        builder.HasKey(p => p.Id);

        // IdentityUser relationship (nullable)
        builder.Property(p => p.IdentityUserId)
               .IsRequired(false);

        // DateOfBirth
        builder.Property(p => p.DateOfBirth)
               .IsRequired();

        // Gender enum as string
        builder.Property(p => p.Gender)
               .HasConversion<string>()
               .IsRequired();

        // FullName value object as owned type
        builder.OwnsOne(p => p.Name, name =>
        {
            name.Property(n => n.FirstName).HasColumnName("FirstName").IsRequired().HasMaxLength(100);
            name.Property(n => n.LastName).HasColumnName("LastName").IsRequired().HasMaxLength(100);
        });

        // Email value object as owned type
        builder.OwnsOne(p => p.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").IsRequired().HasMaxLength(256);
            email.HasIndex(e => e.Value).IsUnique();
        });

        // PhoneNumber value object as owned type
        builder.OwnsOne(p => p.PhoneNumber, phone =>
        {
            phone.Property(ph => ph.Value).HasColumnName("PhoneNumber").IsRequired().HasMaxLength(20);
        });

        // HomeAddress value object as owned type (nullable)
        builder.OwnsOne(p => p.HomeAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("City").HasMaxLength(100);
            address.Property(a => a.LocationMapUrl).HasColumnName("LocationMapUrl").HasMaxLength(500);
        });

        // PersonalImage value object as owned type (nullable)
        builder.OwnsOne(p => p.PersonalImage, image =>
        {
            image.Property(i => i.FileId).HasColumnName("PersonalImageFileId");
        });

        // Optimistic concurrency via shadow property
        builder.Property<byte[]>("RowVersion")
               .IsRowVersion()
               .HasColumnName("RowVersion");
    }
}