using System;
using System.Linq.Expressions;

namespace E_Commerce.Domain.SharedKernel.Interfaces
{
    public interface ISpecification<T>
    {
        /// <summary>
        /// Converts the specification to an expression that can be used in LINQ queries.
        /// </summary>
        Expression<Func<T, bool>> ToExpression();

        /// <summary>
        /// Evaluates whether the specification is satisfied by the given entity.
        /// </summary>
        bool IsSatisfiedBy(T entity);
    }
}