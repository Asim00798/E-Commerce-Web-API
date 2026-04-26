
namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase.ValueObjects
{
    public sealed record TargetEntityInfo
    {
        public Guid EntityId { get; init; }
        public string EntityType { get; init; }

        public TargetEntityInfo(Guid entityId, string entityType)
        {
            if (entityId == Guid.Empty) throw new Exceptions.ComplianceCaseException("TargetEntityInfo ID cannot be empty.");
            if (string.IsNullOrWhiteSpace(entityType)) throw new Exceptions.ComplianceCaseException("TargetEntityInfo type cannot be empty.");

            EntityId = entityId;
            EntityType = entityType;
        }
    }
}
