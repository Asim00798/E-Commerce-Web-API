namespace E_Commerce.Api.DTOs.v1.Profiles.CustomerProfile.Responses;

/// <summary>
/// Public API v1 representation of an aggregated customer profile.
/// Owned by the API layer for v1 contract isolation.
/// </summary>
public sealed class CustomerProfileResponse
{
    public Guid CustomerId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public int TotalOrders { get; init; }
    public decimal TotalSpent { get; init; }
    public double AverageRating { get; init; }
    public int WishlistItemCount { get; init; }
    public DateTime? LastOrderDate { get; init; }
}