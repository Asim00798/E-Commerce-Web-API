#if false
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.BoundedContexts.UserManagement.TenantManagement.Enums;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.TenantManagement.Entities
{
    public sealed class FeatureToggle : BaseEntity, IEntity<FeatureToggle>
    {
        public string Key { get; private set; } = null!;        // e.g. "EnableDiscounts"
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }

        public FeatureToggleStatus Status { get; private set; }

        public bool IsGlobal { get; private set; }               // global feature or tenant-based
        public Guid? TenantId { get; private set; }              // null if global

        public DateTime CreatedOnUtc { get; private set; }
        public DateTime? EnabledOnUtc { get; private set; }
        public DateTime? DisabledOnUtc { get; private set; }

        private FeatureToggle() { }

        private FeatureToggle(
            string key,
            string name,
            string? description,
            bool isGlobal,
            Guid? tenantId)
        {
            ValidateKey(key);
            ValidateName(name);

            Key = key;
            Name = name;
            Description = description;

            IsGlobal = isGlobal;
            TenantId = tenantId;

            Status = FeatureToggleStatus.Disabled;
            CreatedOnUtc = DateTime.UtcNow;
        }

        public static FeatureToggle CreateGlobal(
            string key,
            string name,
            string? description = null)
        {
            return new FeatureToggle(key, name, description, true, null);
        }

        public static FeatureToggle CreateForTenant(
            Guid tenantId,
            string key,
            string name,
            string? description = null)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("TenantId cannot be empty.");

            return new FeatureToggle(key, name, description, false, tenantId);
        }

        // =========================
        // Behavior
        // =========================

        public void Enable()
        {
            if (Status == FeatureToggleStatus.Enabled)
                return;

            Status = FeatureToggleStatus.Enabled;
            EnabledOnUtc = DateTime.UtcNow;
            DisabledOnUtc = null;
        }

        public void Disable()
        {
            if (Status == FeatureToggleStatus.Disabled)
                return;

            Status = FeatureToggleStatus.Disabled;
            DisabledOnUtc = DateTime.UtcNow;
        }

        public void UpdateName(string name)
        {
            ValidateName(name);
            Name = name;
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }

        // =========================
        // Validation
        // =========================

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Feature key cannot be empty.");

            if (key.Length > 100)
                throw new ArgumentException("Feature key cannot exceed 100 characters.");

            if (key.Contains(" "))
                throw new ArgumentException("Feature key cannot contain spaces.");
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Feature name cannot be empty.");

            if (name.Length > 200)
                throw new ArgumentException("Feature name cannot exceed 200 characters.");
        }
    }
}
#endif