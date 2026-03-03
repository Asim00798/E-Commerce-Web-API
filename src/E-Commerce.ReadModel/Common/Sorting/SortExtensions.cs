using System.Linq.Expressions;
using System.Reflection;

namespace E_Commerce.ReadModel.Common.Sorting;

public static class SortExtensions
{
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> source, string? sortBy, bool isAscending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return source;
        }

        var propertyInfo = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (propertyInfo == null)
        {
            // Optionally throw or return unsorted. Returning unsorted is safer to avoid Runtime crashes on bad input.
            return source;
        }

        var param = Expression.Parameter(typeof(T), "item");
        var property = Expression.Property(param, propertyInfo);
        var lambda = Expression.Lambda(property, param);

        string methodName = isAscending ? "OrderBy" : "OrderByDescending";
        var resultExpression = Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), property.Type }, source.Expression, Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(resultExpression);
    }
}
