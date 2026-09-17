using System.ComponentModel.DataAnnotations;
using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Enums;

namespace E_Commerce.Api.DTOs.v1.People.Requests;

/// <summary>
/// API v1 request shape for creating the current user's Person profile.
///
/// Mapped by the controller to <c>CreateMyPersonCommand</c>. The identity
/// link is not part of the request — it is derived from the authenticated
/// user inside the Application handler.
///
/// <see cref="Gender"/> and <see cref="AddressRequest.Type"/> reference the
/// Domain enum directly. This is a deliberate pragmatic choice: they are
/// stable, low-cardinality, and already shared with the Application command
/// input, so a parallel API-layer enum would be pure duplication.
/// </summary>
public sealed class CreatePersonRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? SecondName { get; set; }

    [StringLength(100)]
    public string? ThirdName { get; set; }

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateOnly? DateOfBirth { get; set; }

    [Required]
    public Gender? Gender { get; set; }

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public AddressRequest HomeAddress { get; set; } = null!;
}