using Ambev.DeveloperEvaluation.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Ambev.DeveloperEvaluation.ORM.Querying;

/// <summary>
/// Applies <see cref="QueryOptions"/> (filters, ordering and pagination) to an <see cref="IQueryable{T}"/>,
/// building the expressions dynamically from the property paths.
/// </summary>
public static class QueryableExtensions
{
    private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
    private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!;
    private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!;
    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

    /// <summary>
    /// Applies the filters: groups are combined with AND and the criteria inside a group with OR.
    /// String comparisons are case-insensitive.
    /// </summary>
    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, QueryOptions options)
    {
        foreach (var group in options.FilterGroups)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            Expression? body = null;

            foreach (var criterion in group)
            {
                var predicate = BuildPredicate(parameter, criterion);
                body = body is null ? predicate : Expression.OrElse(body, predicate);
            }

            if (body is not null)
                query = query.Where(Expression.Lambda<Func<T, bool>>(body, parameter));
        }

        return query;
    }

    /// <summary>
    /// Applies the orderings, in the informed sequence.
    /// </summary>
    public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, IReadOnlyList<OrderCriterion> orderings)
    {
        var first = true;

        foreach (var ordering in orderings)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var member = BuildMemberAccess(parameter, ordering.PropertyPath);
            var keySelector = Expression.Lambda(member, parameter);

            var methodName = first
                ? (ordering.Descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy))
                : (ordering.Descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy));

            var call = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { typeof(T), member.Type },
                query.Expression,
                Expression.Quote(keySelector));

            query = query.Provider.CreateQuery<T>(call);
            first = false;
        }

        return query;
    }

    /// <summary>
    /// Applies filters, ordering and pagination, returning the page and the total of items.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, QueryOptions options, CancellationToken cancellationToken = default)
    {
        var filtered = query.ApplyFilters(options);
        var totalItems = await filtered.CountAsync(cancellationToken);

        var items = await filtered
            .ApplyOrdering(options.Orderings)
            .Skip((options.Page - 1) * options.Size)
            .Take(options.Size)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalItems, options.Page, options.Size);
    }

    private static Expression BuildMemberAccess(Expression parameter, string propertyPath) =>
        propertyPath.Split('.').Aggregate<string, Expression>(parameter, (expression, name) => Expression.Property(expression, name));

    private static Expression BuildPredicate(ParameterExpression parameter, FilterCriterion criterion)
    {
        var member = BuildMemberAccess(parameter, criterion.PropertyPath);

        if (member.Type == typeof(string))
        {
            var value = Expression.Constant(((string?)criterion.Value ?? string.Empty).ToLowerInvariant(), typeof(string));
            var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
            var lowerMember = Expression.Call(member, ToLowerMethod);

            Expression comparison = criterion.Operator switch
            {
                FilterOperator.StartsWith => Expression.Call(lowerMember, StartsWithMethod, value),
                FilterOperator.EndsWith => Expression.Call(lowerMember, EndsWithMethod, value),
                FilterOperator.Contains => Expression.Call(lowerMember, ContainsMethod, value),
                FilterOperator.Equal => Expression.Equal(lowerMember, value),
                _ => throw new NotSupportedException($"Operator {criterion.Operator} is not supported for text fields.")
            };

            return Expression.AndAlso(notNull, comparison);
        }

        var constant = Expression.Constant(criterion.Value, member.Type);

        return criterion.Operator switch
        {
            FilterOperator.Equal => Expression.Equal(member, constant),
            FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, constant),
            FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(member, constant),
            _ => throw new NotSupportedException($"Operator {criterion.Operator} is not supported for the field '{criterion.PropertyPath}'.")
        };
    }
}
