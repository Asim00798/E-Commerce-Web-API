namespace E_Commerce.Domain.SharedKernel.Enums
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
