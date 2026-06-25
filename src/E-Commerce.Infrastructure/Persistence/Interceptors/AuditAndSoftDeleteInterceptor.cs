using E_Commerce.Application.Shared.Identity;
using E_Commerce.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace E_Commerce.Infrastructure.Persistence.Interceptors
{
    public class AuditAndSoftDeleteInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUser? _currentUser;

        public AuditAndSoftDeleteInterceptor(ICurrentUser? currentUser = null)
        {
            _currentUser = currentUser;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context is null)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            // Step 1 – Soft‑delete (must come before audit)
            context.ApplySoftDelete();

            // Step 2 – Audit (now captures soft‑deletes as 'Updated')
            context.ApplyAuditLogging(_currentUser?.UserId);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}


