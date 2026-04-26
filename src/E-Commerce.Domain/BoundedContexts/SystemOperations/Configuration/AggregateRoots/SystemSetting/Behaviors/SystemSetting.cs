#if false
using E_Commerce.Domain.BoundedContexts.SystemOperations.Configuration.AggregateRoots.SystemSetting.Entities;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Entities;
using System.Text.Json;

namespace E_Commerce.Domain.BoundedContexts.SystemOperations.Configuration.AggregateRoots.SystemSetting.Behaviors
{
    /// <summary>
    /// Configuration / Settings aggregate root for storing system-wide or module-specific settings
    /// </summary>
    public class SystemSetting : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; } = null!;
        public string ValueJson { get; private set; } = null!; // Stored as JSON for flexibility
        public string Context { get; private set; } = null!; // e.g., Catalog, Admin, Notification
        public string? Description { get; private set; }

        private readonly List<SettingHistory> _history = new();
        public IReadOnlyList<SettingHistory> History => _history.AsReadOnly();

        private SystemSetting() { } // EF Core

        public SystemSetting(string name, object value, string context, string? description = null)
        {
            Name = name;
            ValueJson = JsonSerializer.Serialize(value);
            Context = context;
            Description = description;
        }

        /// <summary>
        /// Update configuration value and store previous version in history
        /// </summary>
        public void UpdateValue(object newValue)
        {
            // Save current state to history
            var previousValue = new SettingHistory(
                configId: this.Id,
                previousValueJson: this.ValueJson,
                updatedAtUtc: DateTime.UtcNow
            );
            _history.Add(previousValue);

            // Update current value
            ValueJson = JsonSerializer.Serialize(newValue);
        }

        /// <summary>
        /// Read value as a typed object
        /// </summary>
        public T GetValue<T>()
        {
            return JsonSerializer.Deserialize<T>(ValueJson)!;
        }
    }
}

#endif