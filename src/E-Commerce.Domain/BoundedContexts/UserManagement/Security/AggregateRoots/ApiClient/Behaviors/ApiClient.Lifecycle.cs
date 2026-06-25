using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Events;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Behaviors
{
    public partial class ApiClient
    {
        public static ApiClient Register(string clientId, string name, DateTime registeredAt)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new BusinessRuleViolationException("Client identifier is required.");

            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("Client name is required.");

            var client = new ApiClient(clientId.Trim(), name.Trim(), registeredAt);
            client.RaiseRegisteredEvent();
            return client;
        }
    }
}
