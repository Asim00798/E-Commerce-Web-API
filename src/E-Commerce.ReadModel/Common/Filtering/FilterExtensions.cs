using System.Linq.Expressions;
using System.Reflection;

namespace E_Commerce.ReadModel.Common.Filtering;

public static class FilterExtensions
{
    public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> source, IEnumerable<FilterRule>? filters)
    {
        if (filters == null || !filters.Any())
        {
            return source;
        }

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.PropertyName) || string.IsNullOrWhiteSpace(filter.Value)) continue;

            var propertyInfo = typeof(T).GetProperty(filter.PropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null) continue;

            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, propertyInfo);
            
            // Handle nullable types or type conversion
            var targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
            object? value;
            try
            {
                 value = Convert.ChangeType(filter.Value, targetType);
            }
            catch
            {
                // If conversion fails, ignore this filter or log it
                continue;
            }

            var constant = Expression.Constant(value, propertyInfo.PropertyType);

            Expression? comparison = null;

            switch (filter.Operation.ToLower())
            {
                case "eq":
                    comparison = Expression.Equal(property, constant);
                    break;
                case "neq":
                    comparison = Expression.NotEqual(property, constant);
                    break;
                case "gt":
                    comparison = Expression.GreaterThan(property, constant);
                    break;
                case "gte":
                    comparison = Expression.GreaterThanOrEqual(property, constant);
                    break;
                case "lt":
                    comparison = Expression.LessThan(property, constant);
                    break;
                case "lte":
                    comparison = Expression.LessThanOrEqual(property, constant);
                    break;
                 case "contains":
                    if (property.Type == typeof(string))
                    {
                        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        if (method != null)
                             comparison = Expression.Call(property, method, constant);
                    }
                    break;
            }

            if (comparison != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(comparison, param);
                source = source.Where(lambda);
            }
        }

        return source;
    }
}
