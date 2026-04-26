namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects
{
    /// <summary>
    /// Represents the expiration date of a document. Handles logic for never-expiring documents as well as past/future expiration checks.
    /// </summary>
    public sealed record ExpirationDate
    {
        public DateTime? Date { get; }
        public bool DoesNotExpire => Date == null;

        private ExpirationDate(DateTime date)
        {
            Date = date.Date; // Ensure only the date component is stored
        }

        public static ExpirationDate On(DateTime date) => new(date);

        public bool IsExpired(DateTime currentDate)
        {
            if (DoesNotExpire) return false;
            return Date < currentDate.Date;
        }

        public override string ToString() => DoesNotExpire ? "No Expiration" : Date?.ToString("yyyy-MM-dd") ?? string.Empty;
    }
}
