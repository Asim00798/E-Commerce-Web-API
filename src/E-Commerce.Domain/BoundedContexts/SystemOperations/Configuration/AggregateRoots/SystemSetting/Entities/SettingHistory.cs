#if false
using E_Commerce.Domain.SharedKernel.Abstract;
using System.Text.Json;

namespace E_Commerce.Domain.BoundedContexts.SystemOperations.Configuration.AggregateRoots.SystemSetting.Entities
{
    /// <summary>
    /// Historical record of configuration changes
    /// </summary>
    public class SettingHistory : BaseEntity
    {
        public Guid SystemConfigurationId { get; private set; }
        public string PreviousValueJson { get; private set; } = null!;
        public DateTime UpdatedAtUtc { get; private set; }

        private SettingHistory() { } // EF Core

        public SettingHistory(Guid configId, string previousValueJson, DateTime updatedAtUtc)
        {
            SystemConfigurationId = configId;
            PreviousValueJson = previousValueJson;
            UpdatedAtUtc = updatedAtUtc;
        }

        /// <summary>
        /// Get value as typed object
        /// </summary>
        public T GetPreviousValue<T>()
        {
            return JsonSerializer.Deserialize<T>(PreviousValueJson)!;
        }
    }
}

#endif