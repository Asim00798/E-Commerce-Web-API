using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.ValueObjects
{
    public sealed record DeviceInfo
    {
        public string? DeviceId { get; }
        public string Platform { get; }
        public string? DeviceName { get; }

        public DeviceInfo(string platform, string? deviceId = null, string? deviceName = null)
        {
            if (string.IsNullOrWhiteSpace(platform))
                throw new BusinessRuleViolationException("Device platform is required.");

            Platform = platform.Trim();
            DeviceId = string.IsNullOrWhiteSpace(deviceId) ? null : deviceId.Trim();
            DeviceName = string.IsNullOrWhiteSpace(deviceName) ? null : deviceName.Trim();
        }
    }
}
