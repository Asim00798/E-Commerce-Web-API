using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.Behaviors
{
    public partial class LoginAttempt
    {
        public void ClassifyRisk(LoginRiskLevel riskLevel, IEnumerable<string>? signals = null)
        {
            if (Outcome == LoginAttemptOutcome.Pending)
                throw new BusinessRuleViolationException(
                    "Login attempt must be resolved before risk classification.");

            RiskLevel = riskLevel;

            if (signals is not null)
            {
                foreach (var signal in signals.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    var normalized = signal.Trim();
                    if (!_signals.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                        _signals.Add(normalized);
                }

                SecuritySignals = _signals.AsReadOnly();
            }
        }
    }
}
