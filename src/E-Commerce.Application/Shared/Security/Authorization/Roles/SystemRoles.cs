namespace E_Commerce.Application.Shared.Security.Authorization.Roles;

/// <summary>
/// System-defined role names.
///
/// Roles are seeded at startup and assigned to users through the authorization
/// module. Every role name referenced anywhere in the codebase — API controller
/// [Authorize(Roles = ...)] attributes, seeding, service implementations, or
/// domain event handlers — must come from a constant here.
///
/// The string values are the authoritative identifiers. They are persisted in
/// the database (<c>AspNetRoles.Name</c>) and compared by name during
/// authorization. Renaming a constant's value is a data migration.
/// </summary>
public static class SystemRoles
{
    // ------------------------------------------------------------------
    // Administrative
    // ------------------------------------------------------------------

    /// <summary>
    /// Full system access. Grants every administrative capability.
    /// </summary>
    public const string Administrator = "Administrator";

    /// <summary>
    /// Customer-facing support operations. Intended for staff who need to
    /// assist customers without full administrative privileges.
    /// </summary>
    public const string Support = "Support";

    // ------------------------------------------------------------------
    // Catalog
    // ------------------------------------------------------------------

    /// <summary>
    /// Catalog management — products, brands, categories, stock.
    /// </summary>
    public const string CatalogManager = "CatalogManager";

    // ------------------------------------------------------------------
    // Shipping operations
    // ------------------------------------------------------------------

    /// <summary>
    /// Dispatches shipments and assigns drivers to delivery routes.
    /// </summary>
    public const string Dispatcher = "Dispatcher";

    /// <summary>
    /// Internal delivery driver. Assigns to specific shipments and records
    /// delivery attempts.
    /// </summary>
    public const string Driver = "Driver";

    /// <summary>
    /// Packaging / warehouse operations — prepares shipments for pickup and
    /// completes return flows.
    /// </summary>
    public const string Packaging = "Packaging";

    // ------------------------------------------------------------------
    // Customer-facing
    // ------------------------------------------------------------------

    /// <summary>
    /// Authenticated customer. Default role assigned to every user that
    /// completes registration. Grants access to cart, order, profile, and
    /// other self-service capabilities.
    /// </summary>
    public const string Customer = "Customer";

    // ------------------------------------------------------------------
    // Employee-facing
    // ------------------------------------------------------------------

    /// <summary>
    /// Authenticated employee. Default role assigned to every user that
    /// completes employee registration. Grants access to employee-facing capabilities.
    /// </summary>
    public const string Employee = "Employee";

    // ------------------------------------------------------------------
    // Aggregate
    // ------------------------------------------------------------------

    /// <summary>
    /// Every role name defined by the system. Used by seeding and by
    /// validation routines that need to iterate all roles.
    /// </summary>
    public static readonly IReadOnlyList<string> All =
    [
        Administrator,
        Support,
        CatalogManager,
        Dispatcher,
        Driver,
        Packaging,
        Customer,
        Employee
    ];
}