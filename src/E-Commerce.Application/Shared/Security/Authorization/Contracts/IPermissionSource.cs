using System.Reflection;

namespace E_Commerce.Application.Shared.Security.Authorization.Contracts;

/// <summary>
/// Marks a class as a source of permission identifiers.
///
/// Implementing classes declare their permissions as public const string fields.
/// The default <see cref="Describe"/> implementation reflects over the
/// implementing type and returns every public const string field — so a new
/// permission is added by declaring one constant. No method body to maintain.
/// </summary>
public interface IPermissionSource
{
    /// <summary>
    /// Returns every permission identifier declared by the implementing class.
    /// </summary>
    IEnumerable<string> Describe() =>
        GetType()
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();
}