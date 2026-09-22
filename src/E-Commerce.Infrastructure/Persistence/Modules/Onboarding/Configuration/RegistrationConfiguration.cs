using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Onboarding.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Registration"/> aggregate.
/// Applies shared auditing, soft-delete conventions, owned value objects,
/// JSON-mapped verification channels, unique indexes, and a RowVersion concurrency token.
/// </summary>
internal sealed class RegistrationConfiguration : BaseEntityConfiguration<Registration>
{
    public override void Configure(EntityTypeBuilder<Registration> builder)
    {
        base.Configure(builder);   // audit + soft-delete + query filter

        builder.ToTable("Registrations", "onboarding");
        builder.HasKey(r => r.Id);

        // Owned value objects
        builder.OwnsOne(r => r.Email, email =>
        {
            email.Property(e => e.Value)
                 .HasColumnName("Email")
                 .IsRequired()
                 .HasMaxLength(256);
            email.HasIndex(e => e.Value).IsUnique();
            email.WithOwner();
        });

        builder.OwnsOne(r => r.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                 .HasColumnName("PhoneNumber")
                 .IsRequired()
                 .HasMaxLength(20);
            phone.HasIndex(p => p.Value).IsUnique();
            phone.WithOwner();
        });

        builder.OwnsOne(r => r.Username, username =>
        {
            username.Property(u => u.Value)
                    .HasColumnName("Username")
                    .IsRequired()
                    .HasMaxLength(100);
            username.HasIndex(u => u.Value).IsUnique();
            username.WithOwner();
        });

        builder.OwnsOne(r => r.PasswordHash, password =>
        {
            password.Property(p => p.Value)
                   .HasColumnName("PasswordHash")
                   .IsRequired();
            password.WithOwner();
        });

        // Verification channels stored as JSON (immutable value objects)
        builder.OwnsOne(r => r.EmailVerification, ev =>
        {
            ev.ToJson("EmailVerification");
        });

        builder.OwnsOne(r => r.PhoneVerification, pv =>
        {
            pv.ToJson("PhoneVerification");
        });

        // Shadow property for optimistic concurrency
        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}