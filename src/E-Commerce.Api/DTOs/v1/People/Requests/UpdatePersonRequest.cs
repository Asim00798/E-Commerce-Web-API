using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.DTOs.v1.People.Requests;

/// <summary>
/// API v1 request shape for updating the current user's Person profile.
///
/// Only phone number, email, and home address are updatable. Name, gender,
/// and date of birth are set at creation and are immutable through this
/// endpoint — changing them is a separate concern not currently exposed.
/// </summary>
public sealed class UpdatePersonRequest
{
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