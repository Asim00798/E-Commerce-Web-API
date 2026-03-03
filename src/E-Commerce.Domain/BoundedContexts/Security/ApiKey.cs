using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.Security
{
    public class ApiKey : BaseEntity
    {
        public Guid ApiClientId { get; set; }
        public ApiClient ApiClient { get; set; } = null!;

        public string Key { get; set; } = string.Empty;
        public DateTimeOffset Expiration { get; set; }
        public bool IsActive { get; set; } = true;

        public override void Validate()
        {
            base.Validate();

            if (ApiClientId == Guid.Empty)
                throw new InvalidOperationException("ApiKey must belong to an ApiClient.");

            if (string.IsNullOrWhiteSpace(Key))
                throw new InvalidOperationException("ApiKey value cannot be empty.");

            if (Expiration <= DateTimeOffset.UtcNow)
                throw new InvalidOperationException("ApiKey expiration must be in the future.");
        }
    }
}
