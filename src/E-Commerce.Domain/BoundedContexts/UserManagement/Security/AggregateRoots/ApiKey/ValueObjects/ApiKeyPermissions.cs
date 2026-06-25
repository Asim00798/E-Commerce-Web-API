using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.ValueObjects
{
    public sealed record ApiKeyPermissions
    {
        private readonly HashSet<string> _permissions;

        public IReadOnlyCollection<string> Permissions => _permissions;

        public ApiKeyPermissions(IEnumerable<string> permissions)
        {
            if (permissions is null)
                throw new ArgumentNullException(nameof(permissions));

            _permissions = permissions
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim().ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (_permissions.Count == 0)
                throw new BusinessRuleViolationException("At least one API key permission is required.");
        }

        public bool HasPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return false;

            var normalized = permission.Trim().ToLowerInvariant();
            return _permissions.Contains("*") || _permissions.Contains(normalized);
        }

        public bool HasAllPermissions(IEnumerable<string> requiredPermissions) =>
            requiredPermissions.All(HasPermission);

        public ApiKeyPermissions WithScope(ApiKeyScope scope)
        {
            var merged = _permissions.Append(scope.Value).Distinct(StringComparer.OrdinalIgnoreCase);
            return new ApiKeyPermissions(merged);
        }
    }
}
