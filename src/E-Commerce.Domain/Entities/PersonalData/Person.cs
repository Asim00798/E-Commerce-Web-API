using E_Commerce.Domain.Entities;
using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Enums;

namespace E_Commerce.Domain.Entities.PersonalData
{
    public class Person : BaseEntity 
    {
        // Personal Info
        public string FirstName { get; set; } = string.Empty;
        public string? SecondName { get; set; }
        public string? ThirdName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; } = Gender.Unspecified;

        // Optional personal image
        public string? ProfileImageUrl { get; set; }

        // Optional foreign keys
        public Guid? AddressId { get; set; }

        // Navigation
        public Address? Address { get; set; }
        public ICollection<Contact>? Contacts { get; set; }

        // Business rules
        public override void Validate()
        {
            base.Validate();

            if (DateOfBirth > DateTime.UtcNow)
                throw new InvalidOperationException("DateOfBirth cannot be in the future.");

            if (DateOfBirth < DateTime.UtcNow.AddYears(-120))
                throw new InvalidOperationException("Person cannot be older than 120 years.");
        }
    }

}
