using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Domain.SharedKernel.Enums
{
   public enum AuditActionType
    {
        Unknown = 0,
        Created = 1,
        Updated = 2,
        Deleted = 3,
        Viewed = 4
    }
}
