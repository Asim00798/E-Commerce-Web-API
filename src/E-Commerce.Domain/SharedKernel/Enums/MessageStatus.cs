using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Domain.SharedKernel.Enums
{
    public enum MessageStatus
    {
        None = 0,
        Sent = 1,
        Failed = 2,
        Pending = 3
    }
}
