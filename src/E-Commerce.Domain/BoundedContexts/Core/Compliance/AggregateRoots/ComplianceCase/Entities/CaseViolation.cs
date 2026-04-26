using System;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase.Entities
{
    public class CaseViolation : BaseEntity
    {
        public ViolationDetail Violation { get; private set; }

        internal CaseViolation(ViolationDetail violation)
        {
            Violation = violation ?? throw new ArgumentNullException(nameof(violation));
        }

        public static CaseViolation Create(ViolationDetail violation)
        {
            return new CaseViolation(violation);
        }
    }
}
