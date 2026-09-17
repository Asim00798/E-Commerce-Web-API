namespace E_Commerce.Api.DTOs.v1.Notifications.Requests
{
    public record RegisterPushDeviceRequest(string FirebaseInstallationId, string Platform);
}
