using E_Commerce.Domain.SharedKernel.Events;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase.Events
{
    public sealed record ComplianceCaseEvaluatedEvent : IDomainEvent
    {
        public Guid CaseId { get; init; }
        public ComplianceStatusEnum FinalStatus { get; init; }
        public int ViolationsCount { get; init; }
        public SeverityLevelEnum MaxSeverity { get; init; }
        public DateTime OccurredOn { get; init; }

        public ComplianceCaseEvaluatedEvent(Guid caseId, ComplianceStatusEnum finalStatus, int violationsCount, SeverityLevelEnum maxSeverity)
        {
            CaseId = caseId;
            FinalStatus = finalStatus;
            ViolationsCount = violationsCount;
            MaxSeverity = maxSeverity;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
