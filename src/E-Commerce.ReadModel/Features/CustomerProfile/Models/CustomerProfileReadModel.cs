namespace E_Commerce.ReadModel.Features.CustomerProfile.Models;

/// <summary>
/// Denormalized read model representing a customer profile with aggregated data.
/// </summary>
public sealed class CustomerProfileReadModel
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