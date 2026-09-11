namespace E_Commerce.Api.DTOs.v1.Shipping.Requests;

public sealed class ReassignDriverRequest
{
    public Guid NewDriverId { get; set; }
}