using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Security
{
    public class ApiClient : BaseEntity
    {
        public string Name { get; set; } = string.Empty;          // Friendly name
        public string ClientId { get; set; } = string.Empty;      // Public ID
        public string ClientSecret { get; set; } = string.Empty;  // Secret for authentication
        public bool IsActive { get; set; } = true;

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("ApiClient Name is required.");

            if (string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret))
                throw new InvalidOperationException("ApiClient ClientId and ClientSecret are required.");
        }
    }
}
