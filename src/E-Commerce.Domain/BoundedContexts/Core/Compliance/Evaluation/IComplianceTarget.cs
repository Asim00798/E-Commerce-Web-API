namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation
{
    /// <summary>
    /// Marker interface for any entity payload that is subjected to compliance rules.
    /// Ensures type safety over raw objects.
    /// IMPORTANT: Implementations MUST be minimal and scenario-specific.
    /// Do NOT use generic 'god' targets, dictionaries, or dynamic bags.
    /// Provide strongly typed properties required strictly for a given evaluation schema.
    /// </summary>
    public interface IComplianceTarget
    {}
}
