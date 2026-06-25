using E_Commerce.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace E_Commerce.Infrastructure.Persistence.Interceptors
{
    public class TimestampInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context is not null)
                context.SetTimestamps();

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
