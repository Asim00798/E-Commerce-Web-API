namespace E_Commerce.Api.DTOs.v1.Profiles.EmployeeProfile.Responses;

/// <summary>
/// Public API v1 representation of an aggregated employee profile.
/// Owned by the API layer for v1 contract isolation.
/// </summary>
public sealed class EmployeeProfileResponse
{
    public Guid EmployeeId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public int ActiveShipments { get; init; }
    public int CompletedShipments { get; init; }
    public double AverageRating { get; init; }
    public DateTime? LastActiveAt { get; init; }
}