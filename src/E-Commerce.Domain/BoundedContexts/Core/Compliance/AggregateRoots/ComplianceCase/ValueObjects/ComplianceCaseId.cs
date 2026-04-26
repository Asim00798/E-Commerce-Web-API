
namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase.ValueObjects
{
    public sealed record ComplianceCaseId
    {
        public Guid Value { get; init; }

        public ComplianceCaseId(Guid value)
        {
            if (value == Guid.Empty)
                throw new Exceptions.ComplianceCaseException("Compliance case ID cannot be empty.");

            Value = value;
        }

        public static ComplianceCaseId New() => new ComplianceCaseId(Guid.NewGuid());
    }
}
