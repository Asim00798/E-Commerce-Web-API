namespace E_Commerce.ReadModel.Common.Filtering;

/// <summary>
/// Represents a single filter rule (field, operator, value) for dynamic query filtering.
/// </summary>
public sealed class FilterRule
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string? Value { get; set; }
}
