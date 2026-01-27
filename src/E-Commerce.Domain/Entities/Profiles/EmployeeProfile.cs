using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Identity;
using E_Commerce.Domain.Entities.PersonalData;

namespace E_Commerce.Domain.Entities.Profiles
{
    public class EmployeeProfile : BaseEntity
    {
        public Guid UserId { get; set; } // Link to User
        public User User { get; set; } = null!;

        public Guid? PersonId { get; set; }
        public Person? Person { get; set; }

        // Employee-specific info
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public DateTimeOffset? HireDate { get; set; }
        public string? Department { get; set; }
        public Address? Address { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (UserId == Guid.Empty)
                throw new InvalidOperationException("EmployeeProfile must have a valid UserId.");

            if (Salary < 0)
                throw new InvalidOperationException("Salary cannot be negative.");
        }
    }
}
