using Domain.BoundedContexts.UserManagement.Security.Entities.LoginAttempt.Behaviors;

namespace Domain.BoundedContexts.UserManagement.Security.Repositories
{
    public interface ILoginAttemptRepository
    {
        Task<LoginAttempt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(LoginAttempt attempt, CancellationToken cancellationToken = default);
        Task UpdateAsync(LoginAttempt attempt, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LoginAttempt>> GetRecentAttemptsAsync(
            Guid userId,
            TimeSpan window,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LoginAttempt>> GetFailedAttemptsAsync(
            string ipAddress,
            TimeSpan window,
            CancellationToken cancellationToken = default);
    }
}
