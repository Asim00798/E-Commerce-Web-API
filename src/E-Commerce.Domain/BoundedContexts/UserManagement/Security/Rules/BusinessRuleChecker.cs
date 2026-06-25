using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.Rules;

namespace Domain.BoundedContexts.UserManagement.Security.Rules
{
    internal static class BusinessRuleChecker
    {
        public static void Check(IBusinessRule rule)
        {
            if (!rule.IsSatisfied())
                throw new BusinessRuleViolationException(rule.Message);
        }
    }
}
