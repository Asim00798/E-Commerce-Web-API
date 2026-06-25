using E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {}
}


