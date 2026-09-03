using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.ValueObjects;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Constants;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Events;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.Behaviors
{
    public sealed partial class Registration : BaseEntity, IAggregateRoot
    {
        public Email Email { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; }
        public Username Username { get; private set; }
        public CredentialHash PasswordHash { get; private set; }

        // Separate verification states for each channel
        public VerificationChannel EmailVerification { get; private set; }
        public VerificationChannel PhoneVerification { get; private set; }

        public DateTime? ExpiresAtUtc { get; private set; }
        public bool RegistrationCompleted { get; private set; }

        // EF Core parameterless constructor
        private Registration() : base()
        {
            Email = null!;
            PhoneNumber = null!;
            Username = null!;
            PasswordHash = null!;
            EmailVerification = VerificationChannel.Empty;
            PhoneVerification = VerificationChannel.Empty;
        }

        public Registration(
            string email,
            string phoneNumber,
            string username,
            string passwordHash,
            DateTime utcNow)
            : base()
        {
            Id = Guid.NewGuid();
            Email = new Email(email.Trim().ToLowerInvariant()); // normalization happens inside Email
            PhoneNumber = new PhoneNumber(phoneNumber);
            Username = new Username(username);
            PasswordHash = new CredentialHash(passwordHash);
            CreatedAt = utcNow;
            ExpiresAtUtc = utcNow.Add(RegistrationConstants.DefaultLifetime);

            EmailVerification = VerificationChannel.Empty;
            PhoneVerification = VerificationChannel.Empty;

            AddDomainEvent(new RegistrationCreatedDomainEvent(
                Id, Email.Value, PhoneNumber.Value, Username.Value));
        }
    }
}