using E_Commerce.Application.BoundedContexts.Finance.Abstractions;
using E_Commerce.Application.BoundedContexts.Finance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Payments;

[ApiController]
[Route("api/payments")]
public sealed class PaymobWebhookController : BaseApiController
{
    private readonly IPaymentWebhookProcessor _webhookProcessor;

    public PaymobWebhookController(IPaymentWebhookProcessor webhookProcessor)
    {
        _webhookProcessor = webhookProcessor;
    }

    [HttpPost("webhook/paymob")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PaymobWebhook(
        [FromHeader(Name = "hmac")] string? hmac,
        CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(ct);

        var result = await _webhookProcessor.ProcessAsync(
            "Paymob",
            payload,
            hmac,
            ct);

        if (result.Succeeded)
        {
            return Ok();
        }

        return result.ErrorType switch
        {
            PaymentWebhookErrorType.Unauthorized => Unauthorized(),
            PaymentWebhookErrorType.Validation => BadRequest(result.Errors),
            PaymentWebhookErrorType.NotFound => NotFound(result.Errors),
            PaymentWebhookErrorType.Conflict => Conflict(result.Errors),
            PaymentWebhookErrorType.Transient => StatusCode(
                StatusCodes.Status500InternalServerError,
                result.Errors),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                result.Errors)
        };
    }
}