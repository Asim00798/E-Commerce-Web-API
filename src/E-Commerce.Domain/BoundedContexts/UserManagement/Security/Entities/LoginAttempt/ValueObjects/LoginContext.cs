using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.ValueObjects;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.ValueObjects
{
    public sealed record LoginContext
    {
        public Guid? UserId { get; }
        public IpAddress IpAddress { get; }
        public DeviceInfo DeviceInfo { get; }
        public string? UserAgent { get; }
        public DateTime AttemptedAt { get; }

        public LoginContext(
            Guid? userId,
            IpAddress ipAddress,
            DeviceInfo deviceInfo,
            string? userAgent,
            DateTime attemptedAt)
        {
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            DeviceInfo = deviceInfo ?? throw new ArgumentNullException(nameof(deviceInfo));

            if (attemptedAt == default)
                throw new BusinessRuleViolationException("Login attempt timestamp is required.");

            UserId = userId;
            UserAgent = NormalizeUserAgent(userAgent);
            AttemptedAt = attemptedAt;
        }

        private static string? NormalizeUserAgent(string? userAgent) =>
            string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim();
    }
}
