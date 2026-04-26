namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects
{
    /// <summary>
    /// Identifies the legal or organisational entity that owns a document.
    /// Combines an owner identifier with an owner category (e.g., User, Merchant, Organisation).
    /// </summary>
    public sealed record DocumentOwner
    {
        public Guid OwnerId { get; }
        public string OwnerType { get; }

        private static readonly HashSet<string> _allowedOwnerTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Brand","Product","User", "Merchant", "Organisation", "Partner", "Admin", "System"
        };

        public DocumentOwner(Guid ownerId, string ownerType)
        {
            ValidateOwnerId(ownerId);
            ValidateOwnerType(ownerType);

            OwnerId = ownerId;
            OwnerType = ownerType.Trim();
        }

        private static void ValidateOwnerId(Guid ownerId)
        {
            if (ownerId == Guid.Empty)
                throw new ArgumentException("OwnerId cannot be an empty GUID.", nameof(ownerId));
        }

        private static void ValidateOwnerType(string ownerType)
        {
            if (string.IsNullOrWhiteSpace(ownerType))
                throw new ArgumentException("OwnerType must not be empty.", nameof(ownerType));

            if (!_allowedOwnerTypes.Contains(ownerType))
                throw new ArgumentException($"'{ownerType}' is not a supported owner type.", nameof(ownerType));
        }

        public bool IsUser() => OwnerType.Equals("User", StringComparison.OrdinalIgnoreCase);
        public bool IsMerchant() => OwnerType.Equals("Merchant", StringComparison.OrdinalIgnoreCase);

        public override string ToString() => $"{OwnerType}:{OwnerId}";
    }
}
