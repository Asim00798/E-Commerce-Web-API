namespace E_Commerce.Api.DTOs.Payments.Requests;

public sealed class RequestRefundRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Reason { get; set; } = string.Empty;
}