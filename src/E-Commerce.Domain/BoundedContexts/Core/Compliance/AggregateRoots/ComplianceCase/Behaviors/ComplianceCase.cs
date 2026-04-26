using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase
{
    public partial class ComplianceCase : BaseEntity, IAggregateRoot
    {
        public ComplianceCaseId CaseId { get; private set; }
        public TargetEntityInfo TargetEntity { get; private set; }
        public ComplianceStatusEnum Status { get; private set; }
        public EvaluationTimestamp? EvaluatedAt { get; private set; }
        public BoundedContext SourceContext { get; private set; }

        private readonly List<CaseViolation> _violations = new();
        public IReadOnlyCollection<CaseViolation> Violations => _violations.AsReadOnly();

        private ComplianceCase(ComplianceCaseId caseId, TargetEntityInfo targetEntity, BoundedContext sourceContext)
        {
            CaseId = caseId ?? throw new ArgumentNullException(nameof(caseId));
            TargetEntity = targetEntity ?? throw new ArgumentNullException(nameof(targetEntity));
            SourceContext = sourceContext;
            Status = ComplianceStatusEnum.Pending;
            Id = caseId.Value;
        }

        public static ComplianceCase Create(TargetEntityInfo targetEntity, BoundedContext sourceContext)
        {
            return new ComplianceCase(ComplianceCaseId.New(), targetEntity, sourceContext);
        }
    }
}
