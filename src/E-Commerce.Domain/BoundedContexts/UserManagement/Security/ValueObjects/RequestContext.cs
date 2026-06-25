using Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.ValueObjects;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.ValueObjects
{
    public sealed record RequestContext
    {
        public Guid? ApiClientId { get; }
        public IpAddress IpAddress { get; }
        public string? UserAgent { get; }
        public DateTime RequestedAt { get; }

        public RequestContext(
            Guid? apiClientId,
            IpAddress ipAddress,
            string? userAgent,
            DateTime requestedAt)
        {
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));

            if (requestedAt == default)
                throw new BusinessRuleViolationException("Request timestamp is required.");

            ApiClientId = apiClientId;
            UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim();
            RequestedAt = requestedAt;
        }
    }
}
