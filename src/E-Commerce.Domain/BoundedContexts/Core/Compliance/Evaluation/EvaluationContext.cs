using E_Commerce.Domain.SharedKernel.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation;

/// <summary>
/// Contains the target entity, source context, and a generic bag of pre‑computed facts.
/// Compliance does not know the meaning of any fact key – it only passes them to specifications.
/// The caller (e.g., Ordering application service) is responsible for populating all facts
/// required by the specifications registered in the system.
/// </summary>
public sealed record EvaluationContext
{
    public Guid TargetEntityId { get; init; }
    public string TargetEntityType { get; init; }
    public IComplianceTarget Target { get; init; }
    public BoundedContext SourceContext { get; init; }

    /// <summary>
    /// Generic dictionary of pre‑computed facts. Keys are defined by the specifications
    /// (e.g., "IsTrademarkValid", "IsCountryAllowed") and values are typically booleans.
    /// Compliance never inspects or relies on specific keys.
    /// </summary>
    public IReadOnlyDictionary<string, object> Facts { get; init; }

    public EvaluationContext(
        Guid targetEntityId,
        string targetEntityType,
        IComplianceTarget target,
        BoundedContext sourceContext,
        IReadOnlyDictionary<string, object>? facts = null)
    {
        if (targetEntityId == Guid.Empty)
            throw new ArgumentException("TargetEntityId cannot be empty.", nameof(targetEntityId));
        if (string.IsNullOrWhiteSpace(targetEntityType))
            throw new ArgumentException("TargetEntityType is required.", nameof(targetEntityType));
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        TargetEntityId = targetEntityId;
        TargetEntityType = targetEntityType;
        Target = target;
        SourceContext = sourceContext;
        Facts = facts ?? new Dictionary<string, object>();
    }
}