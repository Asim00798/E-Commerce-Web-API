using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.Events
{
    public sealed class SuspiciousLoginDetected : DomainEvent
    {
        public Guid LoginAttemptId { get; }
        public Guid? UserId { get; }
        public LoginRiskLevel RiskLevel { get; }
        public string IpAddress { get; }
        public IReadOnlyList<string> Signals { get; }

        public SuspiciousLoginDetected(
            Guid loginAttemptId,
            Guid? userId,
            LoginRiskLevel riskLevel,
            string ipAddress,
            IReadOnlyList<string> signals)
        {
            LoginAttemptId = loginAttemptId;
            UserId = userId;
            RiskLevel = riskLevel;
            IpAddress = ipAddress;
            Signals = signals;
        }
    }
}
