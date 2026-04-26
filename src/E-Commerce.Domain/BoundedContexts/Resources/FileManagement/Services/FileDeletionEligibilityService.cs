using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Policies;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Specifications;
using FileAggregate = E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors.File;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Services
{
    /// <summary>
    /// Pure logic service to decide if a file is eligible for deletion.
    /// </summary>
    public class FileDeletionEligibilityService
    {
        private readonly EligibleForDeletionSpecification _isDeletedSpec = new();

        public bool CanDelete(FileAggregate file, Guid userId, FileRetentionPolicy policy, int retentionDays)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (policy == null) throw new ArgumentNullException(nameof(policy));

            // Must be owner to delete
            if (file.OwnerId != userId) return false;

            // Cannot double delete (Business rule: check if already matches deletion criteria)
            if (_isDeletedSpec.IsSatisfiedBy(file)) return false;

            // Enforcement of retention policy
            if (policy.ShouldRetain(file.CreatedAt, retentionDays))
            {
                // If it should be retained, it cannot be deleted yet
                return false;
            }

            return true;
        }
    }
}

