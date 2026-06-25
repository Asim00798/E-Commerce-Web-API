using Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.ValueObjects;
using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.Behaviors
{
    public partial class LoginAttempt : BaseEntity
    {
        public LoginContext Context { get; private set; } = null!;
        public LoginAttemptOutcome Outcome { get; private set; }
        public LoginRiskLevel RiskLevel { get; private set; }
        public string? FailureReason { get; private set; }
        public IReadOnlyList<string> SecuritySignals { get; private set; } = Array.Empty<string>();
        public bool BruteForceSignalRaised { get; private set; }
        public bool SuspiciousSignalRaised { get; private set; }

        private readonly List<string> _signals = new();

        private LoginAttempt() { }

        private LoginAttempt(LoginContext context)
        {
            Id = Guid.NewGuid();
            Context = context;
            Outcome = LoginAttemptOutcome.Pending;
            RiskLevel = LoginRiskLevel.Unknown;
        }
    }
}
