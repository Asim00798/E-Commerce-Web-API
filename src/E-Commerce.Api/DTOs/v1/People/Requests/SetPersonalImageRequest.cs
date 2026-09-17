using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace E_Commerce.Api.DTOs.v1.People.Requests;

/// <summary>
/// API v1 request shape for uploading the current user's personal image.
///
/// Bound from multipart/form-data. The controller translates the
/// <see cref="IFormFile"/> into the Application-layer <c>FileUpload</c>
/// model — the command never sees ASP.NET Core types.
/// </summary>
public sealed class SetPersonalImageRequest
{
    [Required]
    public IFormFile Image { get; set; } = null!;
}