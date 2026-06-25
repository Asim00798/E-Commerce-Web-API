using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Exceptions;
using Domain.BoundedContexts.UserManagement.Security.Enums;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Behaviors
{
    public partial class ApiKey
    {
        public bool IsActive => Status == ApiKeyStatus.Active;

        public bool IsExpired(DateTime at) => at >= ExpiresAt;

        public void EnsureActive()
        {
            if (!IsActive)
                throw new ApiKeyRevokedException(Id);
        }
    }
}
