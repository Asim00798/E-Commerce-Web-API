using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Payments.Requests;
using E_Commerce.Api.DTOs.v1.Payments.Responses;
using E_Commerce.Application.BoundedContexts.Finance.Commands.InitiatePayment;
using E_Commerce.Application.BoundedContexts.Finance.Commands.RequestRefund;
using E_Commerce.Application.BoundedContexts.Finance.Queries.GetPaymentStatus;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.Payments;

/// <summary>
/// Manages payments and refunds for customer orders.
/// Payment initiation, status queries, and refund requests all operate on
/// resources owned by the caller. Ownership enforcement is performed by the
/// command and query handlers.
/// </summary>
[ApiController]
[Route("api/payments")]
[Authorize(Roles = $"{SystemRoles.Customer},{SystemRoles.Administrator},{SystemRoles.Support}")]
public sealed class PaymentsController : BaseApiController
{
    private readonly ISender _sender;

    public PaymentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Initiates a payment for an existing order.
    /// Returns the provider intention ID and checkout URL.
    /// </summary>
    /// <remarks>
    /// SECURITY NOTE: The current request carries CustomerId, Amount, and
    /// Currency from the client. These are not authoritative and allow
    /// tampering with the payment amount. Until the Application layer is
    /// changed to look up these values from Ordering, any authenticated
    /// customer can initiate a payment for an arbitrary amount.
    /// </remarks>
    [HttpPost("initiate")]
    [ProducesResponseType(typeof(PaymentInitiationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Initiate(
        [FromBody] InitiatePaymentRequest request,
        CancellationToken ct)
    {
        var command = new InitiatePaymentCommand(
            request.OrderId,
            request.CustomerId,
            request.Amount,
            request.Currency,
            request.Method,
            request.ReturnUrl,
            request.CancelUrl,
            request.IdempotencyKey);

        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        var data = result.Data!;

        return Ok(new PaymentInitiationResponse
        {
            Provider = data.Provider,
            IntentionId = data.IntentionId,
            CheckoutUrl = data.CheckoutUrl
        });
    }

    /// <summary>
    /// Gets the current status of a payment. Customers can only query their
    /// own payments; Administrators and Support can query any payment.
    /// </summary>
    [HttpGet("{paymentId:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStatus(
        Guid paymentId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetPaymentStatusQuery(paymentId), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        var dto = result.Data!;

        return Ok(new PaymentResponse
        {
            PaymentId = dto.PaymentId,
            OrderId = dto.OrderId,
            CustomerId = dto.CustomerId,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Status = dto.Status.ToString(),
            Provider = dto.Provider,
            ProviderIntentionId = dto.ProviderIntentionId,
            ProviderTransactionId = dto.ProviderTransactionId,
            CompletedAtUtc = dto.CompletedAtUtc,
            RefundedAmount = dto.RefundedAmount
        });
    }

    /// <summary>
    /// Requests a refund for a captured or partially-refunded payment.
    /// Returns 202 Accepted with the refund ID; refund processing is asynchronous.
    /// Customers can only request refunds for their own payments; Administrators
    /// and Support can request refunds for any payment.
    /// </summary>
    [HttpPost("{paymentId:guid}/refund")]
    [ProducesResponseType(typeof(RefundResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Refund(
        Guid paymentId,
        [FromBody] RequestRefundRequest request,
        CancellationToken ct)
    {
        var command = new RequestRefundCommand(
            paymentId,
            request.Amount,
            request.Currency,
            request.Reason);

        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return Accepted(new RefundResponse
        {
            RefundId = result.Data
        });
    }

    // ------------------------------------------------------------------
    // Error helpers
    // ------------------------------------------------------------------

    private IActionResult ToValidationProblem(string[] errors)
    {
        var modelState = new ModelStateDictionary();
        foreach (var error in errors)
        {
            modelState.AddModelError(string.Empty, error);
        }
        return ValidationProblem(modelState);
    }

    private IActionResult ToNotFoundProblem(string[] errors) =>
        Problem(
            title: "Resource not found.",
            detail: string.Join(" ", errors),
            statusCode: StatusCodes.Status404NotFound);
}