namespace E_Commerce.Application.BoundedContexts.Finance.Models;

public enum PaymentWebhookErrorType
{
    Validation = 1,
    Unauthorized = 2,
    NotFound = 3,
    Conflict = 4,
    Transient = 5,
    Unexpected = 6
}

public sealed class PaymentWebhookCommandResult
{
    public bool Succeeded { get; }
    public string[] Errors { get; }
    public PaymentWebhookErrorType ErrorType { get; }

    private PaymentWebhookCommandResult(
        bool succeeded,
        IEnumerable<string> errors,
        PaymentWebhookErrorType errorType)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        ErrorType = errorType;
    }

    public static PaymentWebhookCommandResult Success()
        => new(true, Array.Empty<string>(), PaymentWebhookErrorType.Unexpected);

    public static PaymentWebhookCommandResult Failure(
        string error,
        PaymentWebhookErrorType errorType)
        => new(false, new[] { error }, errorType);

    public static PaymentWebhookCommandResult Failure(
        IEnumerable<string> errors,
        PaymentWebhookErrorType errorType)
        => new(false, errors, errorType);
}