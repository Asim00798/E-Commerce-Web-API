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
        base.Configure(builder); // audit fields, soft-delete columns, global query filter

        // ------------------------------------------------------------------
        // Table
        // ------------------------------------------------------------------
        builder.ToTable("People", "user");

        // ------------------------------------------------------------------
        // Primary key
        // ------------------------------------------------------------------
        builder.HasKey(p => p.Id);

        // ------------------------------------------------------------------
        // IdentityUser link (nullable)
        //
        // Filtered unique index: at most one active Person per identity user.
        // The filter excludes nulls (unlinked persons) and soft-deleted rows,
        // so a soft-deleted Person does not block the same identity user from
        // being linked to a new Person later.
        // ------------------------------------------------------------------
        builder.Property(p => p.IdentityUserId)
            .IsRequired(false);

        builder.HasIndex(p => p.IdentityUserId)
            .IsUnique()
            .HasFilter("[IdentityUserId] IS NOT NULL AND [IsDeleted] = 0")
            .HasDatabaseName("IX_People_IdentityUserId_Unique");

        // ------------------------------------------------------------------
        // DateOfBirth
        // ------------------------------------------------------------------
        builder.Property(p => p.DateOfBirth)
            .IsRequired();

        // ------------------------------------------------------------------
        // Gender (enum stored as string for schema stability)
        // ------------------------------------------------------------------
        builder.Property(p => p.Gender)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        // ------------------------------------------------------------------
        // FullName value object (owned) — all four components mapped explicitly
        // ------------------------------------------------------------------
        builder.OwnsOne(p => p.Name, name =>
        {
            name.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .IsRequired()
                .HasMaxLength(100);

            name.Property(n => n.SecondName)
                .HasColumnName("SecondName")
                .IsRequired(false)
                .HasMaxLength(100);

            name.Property(n => n.ThirdName)
                .HasColumnName("ThirdName")
                .IsRequired(false)
                .HasMaxLength(100);

            name.Property(n => n.LastName)
                .HasColumnName("LastName")
                .IsRequired()
                .HasMaxLength(100);
        });

        // ------------------------------------------------------------------
        // Email value object (owned) — unique per active Person.
        //
        // The filtered unique index excludes soft-deleted rows so a user who
        // deletes their profile can later re-create it with the same email.
        // ------------------------------------------------------------------
        builder.OwnsOne(p => p.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(256);

            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_People_Email_Unique");
        });

        // ------------------------------------------------------------------
        // PhoneNumber value object (owned)
        // ------------------------------------------------------------------
        builder.OwnsOne(p => p.PhoneNumber, phone =>
        {
            phone.Property(ph => ph.Value)
                .HasColumnName("PhoneNumber")
                .IsRequired()
                .HasMaxLength(20);
        });

        // ------------------------------------------------------------------
        // HomeAddress value object (owned, nullable) — all four components
        // mapped explicitly, including the AddressType enum.
        // ------------------------------------------------------------------
        builder.OwnsOne(p => p.HomeAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Street")
                .IsRequired(false)
                .HasMaxLength(200);

            address.Property(a => a.City)
                .HasColumnName("City")
                .IsRequired(false)
                .HasMaxLength(100);

            address.Property(a => a.Type)
                .HasColumnName("AddressType")
                .HasConversion<string>()
                .IsRequired(false)
                .HasMaxLength(20);

            address.Property(a => a.LocationMapUrl)
                .HasColumnName("LocationMapUrl")
                .IsRequired(false)
                .HasMaxLength(500);
        });

        // ------------------------------------------------------------------
        // PersonalImage value object (owned, nullable)
        // ------------------------------------------------------------------
        builder.OwnsOne(p => p.PersonalImage, image =>
        {
            image.Property(i => i.FileId)
                .HasColumnName("PersonalImageFileId")
                .IsRequired(false);
        });

        // ------------------------------------------------------------------
        // Optimistic concurrency via shadow RowVersion property
        // ------------------------------------------------------------------
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .HasColumnName("RowVersion");
    }
}