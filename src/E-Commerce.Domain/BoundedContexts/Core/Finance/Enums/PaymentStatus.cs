#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.Enums
{
    public enum PaymentStatus
    {
        None = 0,
        Pending = 1,
        Authorized = 2,
        Captured = 3,
        Completed = 4,
        Failed = 5,
        Refunded = 6,
        PartiallyRefunded = 7,
        Cancelled = 8,
        Settled = 9,
        TimedOut = 10,
        Declined = 11
    }
}

#endif