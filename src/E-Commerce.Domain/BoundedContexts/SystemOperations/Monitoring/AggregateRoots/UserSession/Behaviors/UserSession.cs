#if false
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.SystemOperations.Monitoring.AggregateRoots.UserSession.Behaviors
{
    /// <summary>
    /// Tracks user login sessions for auditing, monitoring, and security.
    /// Technical/operational entity.
    /// </summary>
    public class UserSession : BaseEntity, IEntity<UserSession>
    {
        /// <summary>
        /// ID of the user associated with this session
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Unique session token or identifier
        /// </summary>
        public string SessionToken { get; private set; } = null!;

        /// <summary>
        /// IP address of the user
        /// </summary>
        public string? IpAddress { get; private set; }

        /// <summary>
        /// Device or browser information
        /// </summary>
        public string? UserAgent { get; private set; }

        /// <summary>
        /// When the session started
        /// </summary>
        public DateTime StartedAtUtc { get; private set; }

        /// <summary>
        /// When the session ended (null if active)
        /// </summary>
        public DateTime? EndedAtUtc { get; private set; }

        /// <summary>
        /// Indicates if session is currently active
        /// </summary>
        public bool IsActive => EndedAtUtc == null;

        private UserSession() { } // EF Core

        public UserSession(Guid userId, string sessionToken, string? ipAddress = null, string? userAgent = null)
        {
            UserId = userId;
            SessionToken = sessionToken;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            StartedAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Ends the session
        /// </summary>
        public void EndSession()
        {
            EndedAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates session details (e.g., IP/UserAgent) during active session
        /// </summary>
        public void UpdateSessionDetails(string? ipAddress = null, string? userAgent = null)
        {
            if (ipAddress != null) IpAddress = ipAddress;
            if (userAgent != null) UserAgent = userAgent;
        }
    }
}
#endif