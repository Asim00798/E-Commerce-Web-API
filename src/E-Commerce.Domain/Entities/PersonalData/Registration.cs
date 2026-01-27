using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Identity;
using E_Commerce.Domain.Enums;

namespace E_Commerce.Domain.Entities.PersonalData
{
    public class Registration : BaseEntity
    {
        public Guid PersonId { get; set; } // Required FK → Person
        public Person Person { get; set; } = null!;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

        // Navigation to the created User (1–1). User is created after Registration is saved.
        public User? User { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (PersonId == Guid.Empty)
                throw new InvalidOperationException("Registration must reference a valid Person.");
        }
    }

}
