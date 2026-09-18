namespace E_Commerce.Application.Shared.Security.Authorization.Roles;

/// <summary>
/// System-defined role name constants.
///
/// Every role name referenced anywhere in the codebase — API controller
/// <c>[Authorize(Roles = ...)]</c> attributes, role classes, or domain event
/// handlers — must come from a constant here.
///
/// The string values are the authoritative identifiers. They are persisted
/// in <c>AspNetRoles.Name</c> and compared by name during authorization.
/// Renaming a constant's value is a data migration.
///
/// Which roles exist is declared by the <c>IRole</c> implementations in this
/// folder. This class exists only to provide the compile-time constants those
/// implementations reference.
/// </summary>
public static class SystemRoles
{
    public const string Administrator = "Administrator";
    public const string Support = "Support";
    public const string Employee = "Employee";
    public const string CatalogManager = "CatalogManager";
    public const string Dispatcher = "Dispatcher";
    public const string Driver = "Driver";
    public const string Packaging = "Packaging";
    public const string Customer = "Customer";
}