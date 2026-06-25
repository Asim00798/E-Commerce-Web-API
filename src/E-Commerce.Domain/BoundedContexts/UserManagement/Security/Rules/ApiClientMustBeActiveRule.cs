using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Rules;

namespace Domain.BoundedContexts.UserManagement.Security.Rules
{
    internal sealed class ApiClientMustBeActiveRule : IBusinessRule
    {
        private readonly ApiClientStatus _status;
        private readonly string _clientId;

        public ApiClientMustBeActiveRule(ApiClientStatus status, string clientId)
        {
            _status = status;
            _clientId = clientId;
        }

        public bool IsSatisfied() => _status == ApiClientStatus.Active;

        public string Message => $"API client '{_clientId}' must be active.";
    }
}
