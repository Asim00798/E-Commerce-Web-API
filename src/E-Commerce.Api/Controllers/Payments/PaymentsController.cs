using E_Commerce.Application.BoundedContexts.Finance.Commands.InitiatePayment;
using E_Commerce.Application.BoundedContexts.Finance.Commands.RequestRefund;
using E_Commerce.Application.BoundedContexts.Finance.Queries.GetPaymentStatus;
using E_Commerce.Api.DTOs.Payments.Requests;
using E_Commerce.Api.DTOs.Payments.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Payments;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : BaseApiController
{
    private const string PaymentsReadPolicy = "Permission:Payments.Read";
    private const string PaymentsRefundPolicy = "Permission:Payments.Refund";

    private readonly ISender _sender;

    public PaymentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("initiate")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentInitiationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        {
            return BadRequest(result.Errors);
        }

        var data = result.Data!;

        var response = new PaymentInitiationResponse
        {
            Provider = data.Provider,
            IntentionId = data.IntentionId,
            CheckoutUrl = data.CheckoutUrl
        };

        return Ok(response);
    }

    [HttpGet("{paymentId:guid}")]
    [Authorize(Policy = PaymentsReadPolicy)]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(
        Guid paymentId,
        CancellationToken ct)
    {
        var query = new GetPaymentStatusQuery(paymentId);

        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
        {
            return NotFound(result.Errors);
        }

        var dto = result.Data!;

        var response = new PaymentResponse
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
        };

        return Ok(response);
    }

    [HttpPost("{paymentId:guid}/refund")]
    [Authorize(Policy = PaymentsRefundPolicy)]
    [ProducesResponseType(typeof(RefundResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        {
            return BadRequest(result.Errors);
        }

        var response = new RefundResponse
        {
            RefundId = result.Data
        };

        return Accepted(response);
    }
}