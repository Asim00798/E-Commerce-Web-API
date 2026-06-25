using Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.Behaviors;
using Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.ValueObjects;
using Domain.BoundedContexts.UserManagement.Security.Enums;

namespace Domain.BoundedContexts.UserManagement.Security.DomainServices
{
    public sealed class SecurityAnalyzer
    {
        public LoginRiskLevel Analyze(
            LoginContext context,
            IReadOnlyList<LoginAttempt> recentAttempts,
            IReadOnlyList<string>? knownDeviceIds = null,
            IReadOnlyList<string>? knownIpAddresses = null)
        {
            ArgumentNullException.ThrowIfNull(context);

            var signals = new List<string>();
            var score = 0;

            if (context.UserId is null)
            {
                signals.Add("unknown_user");
                score += 1;
            }

            if (recentAttempts.Count(a => a.Outcome == LoginAttemptOutcome.Failed) >= 3)
            {
                signals.Add("recent_failures");
                score += 2;
            }

            if (knownIpAddresses is not null &&
                !knownIpAddresses.Contains(context.IpAddress.Value, StringComparer.OrdinalIgnoreCase))
            {
                signals.Add("unknown_ip");
                score += 2;
            }

            if (knownDeviceIds is not null &&
                context.DeviceInfo.DeviceId is not null &&
                !knownDeviceIds.Contains(context.DeviceInfo.DeviceId, StringComparer.OrdinalIgnoreCase))
            {
                signals.Add("unknown_device");
                score += 2;
            }

            if (IsOffHours(context.AttemptedAt))
            {
                signals.Add("off_hours");
                score += 1;
            }

            return score switch
            {
                0 => LoginRiskLevel.Low,
                1 or 2 => LoginRiskLevel.Medium,
                3 or 4 => LoginRiskLevel.High,
                _ => LoginRiskLevel.Critical
            };
        }

        public IReadOnlyList<string> CollectSignals(
            LoginContext context,
            IReadOnlyList<LoginAttempt> recentAttempts,
            IReadOnlyList<string>? knownDeviceIds = null,
            IReadOnlyList<string>? knownIpAddresses = null)
        {
            var signals = new List<string>();

            if (context.UserId is null)
                signals.Add("unknown_user");

            if (recentAttempts.Count(a => a.Outcome == LoginAttemptOutcome.Failed) >= 3)
                signals.Add("recent_failures");

            if (knownIpAddresses is not null &&
                !knownIpAddresses.Contains(context.IpAddress.Value, StringComparer.OrdinalIgnoreCase))
                signals.Add("unknown_ip");

            if (knownDeviceIds is not null &&
                context.DeviceInfo.DeviceId is not null &&
                !knownDeviceIds.Contains(context.DeviceInfo.DeviceId, StringComparer.OrdinalIgnoreCase))
                signals.Add("unknown_device");

            if (IsOffHours(context.AttemptedAt))
                signals.Add("off_hours");

            return signals;
        }

        private static bool IsOffHours(DateTime attemptedAt)
        {
            var hour = attemptedAt.Hour;
            return hour is >= 0 and < 6;
        }
    }
}
