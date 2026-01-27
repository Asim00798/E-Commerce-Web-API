using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Administration;
using E_Commerce.Domain.Enums;

namespace E_Commerce.Domain.Entities.PersonalData
{
    public class Contact : BaseEntity
    {
        public Guid PersonId { get; set; }
        public Person Person { get; set; } = null!;

        public ContactType Type { get; set; } = ContactType.Phone;
        public string Value { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;

        // Navigation
        public ICollection<Notification>? Notifications { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Value))
                throw new InvalidOperationException("Contact value is required.");

            if (!Enum.IsDefined(typeof(ContactType), Type))
                throw new InvalidOperationException("Invalid contact type.");

            if (Value.Length > 200)
                throw new InvalidOperationException("Contact value cannot exceed 200 characters.");

            // Optional stricter checks
            if (Type == ContactType.Email && !Value.Contains("@"))
                throw new InvalidOperationException("Email contact must contain '@' symbol.");
        }
    }
}
