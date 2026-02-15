using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.ValueObjects
{
    public sealed record CategoryInfo
    {
        public string Name { get; init; }
        public string? Description { get; init; }

        public CategoryInfo(string name, string? description = null)
        {
            Name = ValidateName(name);
            Description = ValidateDescription(description);
        }

        /*
         Enforces only local invariants of the Value Object.
         This method assumes all business-context rules
         (aggregate state, cross-entity constraints, workflows)
         were already validated by the owning Aggregate Root.
        */
        internal CategoryInfo Rename(string newName)
        {
            newName = ValidateName(newName);

            if (newName == Name) return this;

            return this with { Name = newName };
        }

        public CategoryInfo UpdateDescription(string? newDescription)
        {
            newDescription = ValidateDescription(newDescription);

            if (newDescription == Description) return this;

            return this with { Description = newDescription };
        }

        // ----------------------
        // Private helper methods
        // ----------------------
        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("Category name cannot be empty.");
            return name.Trim();
        }

        private static string? ValidateDescription(string? description)
        {
            if (description != null && description.Length > 500)
                throw new BusinessRuleViolationException("Description cannot exceed 500 characters.");
            return description?.Trim();
        }
    }
}
