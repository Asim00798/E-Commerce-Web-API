namespace E_Commerce.Api.DTOs.Shipping.Requests;

public sealed class ReassignDriverRequest
{
    public Guid NewDriverId { get; set; }
}