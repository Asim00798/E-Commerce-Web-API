using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Exceptions;
using Domain.BoundedContexts.UserManagement.Security.Enums;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Behaviors
{
    public partial class ApiClient
    {
        public bool IsOperational => Status == ApiClientStatus.Active;

        public void EnsureOperational()
        {
            if (!IsOperational)
                throw new ApiClientNotActiveException(ClientId);
        }

        public void EnsureNotSuspended()
        {
            if (Status == ApiClientStatus.Suspended)
                throw new ApiClientAlreadySuspendedException(ClientId);
        }
    }
}
