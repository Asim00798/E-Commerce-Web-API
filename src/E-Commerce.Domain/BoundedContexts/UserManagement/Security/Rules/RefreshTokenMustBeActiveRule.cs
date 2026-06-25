using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Rules;

namespace Domain.BoundedContexts.UserManagement.Security.Rules
{
    internal sealed class RefreshTokenMustBeActiveRule : IBusinessRule
    {
        private readonly RefreshTokenStatus _status;

        public RefreshTokenMustBeActiveRule(RefreshTokenStatus status) => _status = status;

        public bool IsSatisfied() => _status == RefreshTokenStatus.Active;

        public string Message => "Refresh token must be active.";
    }
}
