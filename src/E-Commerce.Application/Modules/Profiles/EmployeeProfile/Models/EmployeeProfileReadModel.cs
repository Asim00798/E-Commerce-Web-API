namespace E_Commerce.Application.Modules.Profiles.EmployeeProfile.Models;

/// <summary>
/// Denormalized read model representing an employee profile with role and assignment stats.
/// </summary>
public sealed class EmployeeProfileReadModel
{
    public Guid EmployeeId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public int ActiveShipments { get; init; }
    public int CompletedShipments { get; init; }
    public double AverageRating { get; init; }
    public DateTime? LastActiveAt { get; init; }
}