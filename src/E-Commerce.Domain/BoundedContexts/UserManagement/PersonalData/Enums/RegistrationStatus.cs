#if false
namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.Enums
{
    public enum RegistrationStatus
    {
        None = 0,

        // Lifecycle
        Started = 1,
        Submitted = 2,
        Verified = 3,

        // Terminal outcomes
        Completed = 10,
        Rejected = 11,
        Cancelled = 12,
        Expired = 13,

        // Technical failure (not a business decision)
        Failed = 99
    }
}

#endif