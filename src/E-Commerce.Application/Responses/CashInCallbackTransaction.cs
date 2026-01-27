using E_Commerce.Domain.Entities;
using System.Text.Json.Serialization;

namespace E_Commerce.Domain.Responses
{
    public class CustomCashInCallbackTransaction
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("integration_id")]
        public string? IntegrationId { get; set; }

        [JsonPropertyName("order_id")]
        public string? OrderId { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("amount_cents")]
        public decimal? AmountCents { get; set; }

        [JsonPropertyName("owner")]
        public string? Owner { get; set; }

        [JsonPropertyName("success")]
        public bool? Success { get; set; }

        [JsonPropertyName("is_voided")]
        public bool? IsVoided { get; set; }

        [JsonPropertyName("is_refunded")]
        public bool? IsRefunded { get; set; }

        [JsonPropertyName("is_capture")]
        public bool? IsCapture { get; set; }

        [JsonPropertyName("is_auth")]
        public bool? IsAuth { get; set; }

        [JsonPropertyName("is_3d_secure")]
        public bool? Is3dSecure { get; set; }

        [JsonPropertyName("is_standalone_payment")]
        public bool? IsStandalonePayment { get; set; }

        [JsonPropertyName("pending")]
        public bool? Pending { get; set; }

        [JsonPropertyName("error_occured")]
        public string? ErrorOccured { get; set; }

        [JsonPropertyName("order")]
        public Order? Order { get; set; }
    }

}
