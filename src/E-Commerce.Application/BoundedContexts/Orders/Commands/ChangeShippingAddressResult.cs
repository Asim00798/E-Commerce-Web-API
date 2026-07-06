namespace E_Commerce.Application.Orders.Commands;

/// <summary>
/// Outcome of the ChangeShippingAddress command.
/// </summary>
public class ChangeShippingAddressResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ChangeShippingAddressResult Success(string message = "Shipping address updated.")
        => new() { IsSuccess = true, Message = message };
}