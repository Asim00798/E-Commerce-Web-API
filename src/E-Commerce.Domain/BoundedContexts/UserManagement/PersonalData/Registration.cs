using E_Commerce.Domain.Events.PersonalData.Registration;
using E_Commerce.Domain.BoundedContexts.UserManagement.Identity;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Enums;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData
{
    public class Registration : BaseEntity
    {
        public Guid PersonId { get; private set; }
        public Person Person { get; private set; } = null!;
        public DateTime RegisteredAt { get; private set; }
        public RegistrationStatus Status { get; private set; }

        public User? User { get; private set; }

        private Registration() { } // EF

        public Registration(Person person)
        {
            Person = person ?? throw new ArgumentNullException(nameof(person));
            PersonId = person.Id;
            RegisteredAt = DateTime.UtcNow;
            Status = RegistrationStatus.Started;
        }

        // ---------------------------
        // Lifecycle transitions
        // ---------------------------
        public void Complete()
        {
            if (Status != RegistrationStatus.Verified)
                throw new BusinessRuleViolationException("Only verified registrations can be completed.");

            Status = RegistrationStatus.Completed;

            AddDomainEvent(new RegistrationCompleted(Id, PersonId));
        }

        // Allow rejection from Submitted or Verified states
        public void Reject(string reason)
        {
            if (Status != RegistrationStatus.Submitted &&
                Status != RegistrationStatus.Verified)
                throw new BusinessRuleViolationException("Only active registrations can be rejected.");

            Status = RegistrationStatus.Rejected;
            AddDomainEvent(new RegistrationRejected(Id, PersonId, reason));
        }

        // Allow cancellation from any non-terminal state
        public void Cancel()
        {
            if (Status == RegistrationStatus.Completed)
                throw new BusinessRuleViolationException("Completed registration cannot be cancelled.");

            if (IsTerminal()) return;

            Status = RegistrationStatus.Cancelled;
            AddDomainEvent(new RegistrationCancelled(Id, PersonId));
        }

        //Expire when registration is not completed within a certain time frame (e.g., 7 days)
        public void Expire()
        {
            // Only expire if registration is still active (not completed, rejected, or cancelled)
            if (IsTerminal()) return;

            Status = RegistrationStatus.Expired;
            AddDomainEvent(new RegistrationExpired(Id, PersonId, RegisteredAt));
        }

        //Terminal states: Completed, Rejected, Cancelled, Expired
        private bool IsTerminal() =>
            Status == RegistrationStatus.Completed ||
            Status == RegistrationStatus.Rejected ||
            Status == RegistrationStatus.Cancelled ||
            Status == RegistrationStatus.Expired;
    }

}



