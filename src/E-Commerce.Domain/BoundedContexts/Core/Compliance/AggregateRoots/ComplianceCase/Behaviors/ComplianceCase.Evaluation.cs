using E_Commerce.Domain.BoundedContexts.Core.Compliance.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase.Events;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase
{
    public partial class ComplianceCase
    {
        /// <summary>
        /// Evaluates external results and transitions the state of the compliance case.
        /// This enforces aggregate boundaries according to strict DDD.
        /// If no results are provided, the case is safely marked as Compliant.
        /// </summary>
        public void Evaluate(IEnumerable<ComplianceResult> results, EvaluationTimestamp timestamp)
        {
            GuardAgainstNullResults(results);
            GuardAgainstNullTimestamp(timestamp);
            GuardAgainstReEvaluation();

            var resultsList = results.ToList();
            var hasViolations = ProcessResults(resultsList);
            TransitionState(hasViolations, timestamp);
            RaiseEvaluationEvent();
        }

        // ------------------------ Private Guard Methods ------------------------

        private void GuardAgainstNullResults(IEnumerable<ComplianceResult> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));
        }

        private void GuardAgainstNullTimestamp(EvaluationTimestamp timestamp)
        {
            if (timestamp == null)
                throw new ArgumentNullException(nameof(timestamp));
        }

        private void GuardAgainstReEvaluation()
        {
            if (Status != ComplianceStatusEnum.Pending)
                throw new ComplianceCaseException($"Cannot evaluate case in '{Status}' status. Only Pending cases can be evaluated.");
        }

        // ------------------------ Private Processing Methods ------------------------

        private bool ProcessResults(IEnumerable<ComplianceResult> results)
        {
            bool hasViolations = false;

            if(results != null)
            {
                foreach (var result in results)
                {
                    if (!result.IsCompliant)
                    {
                        ValidateViolationDetails(result);
                        _violations.Add(CaseViolation.Create(result.Violation!));
                        hasViolations = true;
                    }
                }
            }

            return hasViolations;
        }

        private void ValidateViolationDetails(ComplianceResult result)
        {
            if (result.Violation == null)
                throw new ComplianceCaseException("Compliance violation details must be provided if the result is not compliant.");
        }

        // ------------------------ State Transition ------------------------

        private void TransitionState(bool hasViolations, EvaluationTimestamp timestamp)
        {
            Status = hasViolations ? ComplianceStatusEnum.NonCompliant : ComplianceStatusEnum.Compliant;
            EvaluatedAt = timestamp;
        }

        // ------------------------ Domain Event ------------------------

        private void RaiseEvaluationEvent()
        {
            int violationsCount = _violations.Count;
            SeverityLevelEnum maxSeverity = violationsCount > 0
                ? _violations.Max(v => v.Violation.SeverityLevel)
                : SeverityLevelEnum.Low;

            var evaluationEvent = new ComplianceCaseEvaluatedEvent(CaseId.Value, Status, violationsCount, maxSeverity);
            AddDomainEvent(evaluationEvent);
        }
    }
}