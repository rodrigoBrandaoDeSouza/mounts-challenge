using Ambev.DeveloperEvaluation.Domain.Common;
using FluentValidation;
using FluentValidation.Results;
using System.Globalization;
using System.Reflection;

namespace Ambev.DeveloperEvaluation.Application.Common.Querying;

/// <summary>
/// Translates the list parameters of the General API definitions into <see cref="QueryOptions"/>:
/// <list type="bullet">
/// <item><c>_page</c> / <c>_size</c>: pagination (defaults 1 and 10);</item>
/// <item><c>_order</c>: e.g. <c>"price desc, title asc"</c> (ascending when the direction is omitted);</item>
/// <item><c>field=value</c>: equality; for strings, <c>*</c> before/after the value means partial match;</item>
/// <item><c>_minField</c> / <c>_maxField</c>: ranges for numeric and date fields.</item>
/// </list>
/// </summary>
public static class QueryOptionsParser
{
    private const string MinPrefix = "_min";
    private const string MaxPrefix = "_max";
    private const char Wildcard = '*';

    /// <exception cref="ValidationException">When a parameter is invalid.</exception>
    public static QueryOptions Parse<TEntity>(ListQuery query, QueryFieldMap<TEntity> fieldMap)
    {
        var failures = new List<ValidationFailure>();

        var page = query.Page ?? QueryOptions.DefaultPage;
        if (page < 1)
            failures.Add(new ValidationFailure("_page", "The '_page' parameter must be greater than or equal to 1."));

        var size = query.Size ?? QueryOptions.DefaultSize;
        if (size < 1 || size > QueryOptions.MaxSize)
            failures.Add(new ValidationFailure("_size", $"The '_size' parameter must be between 1 and {QueryOptions.MaxSize}."));

        var orderings = ParseOrder(query.Order, fieldMap, failures);
        var filterGroups = ParseFilters(query.Filters, fieldMap, failures);

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return new QueryOptions
        {
            Page = page,
            Size = size,
            Orderings = orderings,
            FilterGroups = filterGroups
        };
    }

    private static IReadOnlyList<OrderCriterion> ParseOrder<TEntity>(string? order, QueryFieldMap<TEntity> fieldMap, List<ValidationFailure> failures)
    {
        var value = order?.Trim().Trim('"', '\'').Trim();
        if (string.IsNullOrEmpty(value))
            return fieldMap.DefaultOrdering;

        var orderings = new List<OrderCriterion>();

        foreach (var part in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = tokens[0];

            if (!fieldMap.TryGetPropertyPath(field, out var propertyPath))
            {
                failures.Add(new ValidationFailure("_order", $"The field '{field}' cannot be used for ordering."));
                continue;
            }

            var descending = false;
            if (tokens.Length == 2)
            {
                if (tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase))
                    descending = true;
                else if (!tokens[1].Equals("asc", StringComparison.OrdinalIgnoreCase))
                    failures.Add(new ValidationFailure("_order", $"Invalid direction '{tokens[1]}' for field '{field}'. Use 'asc' or 'desc'."));
            }
            else if (tokens.Length > 2)
            {
                failures.Add(new ValidationFailure("_order", $"Invalid ordering expression '{part}'."));
            }

            orderings.Add(new OrderCriterion(propertyPath, descending));
        }

        return orderings;
    }

    private static IReadOnlyList<IReadOnlyList<FilterCriterion>> ParseFilters<TEntity>(
        IDictionary<string, string[]> filters,
        QueryFieldMap<TEntity> fieldMap,
        List<ValidationFailure> failures)
    {
        var groups = new List<IReadOnlyList<FilterCriterion>>();

        foreach (var (key, values) in filters)
        {
            var isMin = key.StartsWith(MinPrefix, StringComparison.OrdinalIgnoreCase) && key.Length > MinPrefix.Length;
            var isMax = key.StartsWith(MaxPrefix, StringComparison.OrdinalIgnoreCase) && key.Length > MaxPrefix.Length;
            var field = isMin || isMax ? key[MinPrefix.Length..] : key;

            if (!fieldMap.TryGetPropertyPath(field, out var propertyPath))
            {
                failures.Add(new ValidationFailure(key, $"The field '{field}' cannot be used as a filter."));
                continue;
            }

            var propertyType = ResolvePropertyType(typeof(TEntity), propertyPath);
            var group = new List<FilterCriterion>();

            foreach (var rawValue in values.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                var value = rawValue.Trim();

                if (isMin || isMax)
                {
                    if (!IsRangeType(propertyType))
                    {
                        failures.Add(new ValidationFailure(key, $"Range filters are only supported for numeric and date fields ('{field}')."));
                        continue;
                    }

                    if (TryConvert(value, propertyType, out var converted))
                        group.Add(new FilterCriterion(propertyPath, isMin ? FilterOperator.GreaterThanOrEqual : FilterOperator.LessThanOrEqual, converted));
                    else
                        failures.Add(new ValidationFailure(key, $"The value '{value}' is not valid for the field '{field}'."));

                    continue;
                }

                if (propertyType == typeof(string))
                {
                    group.Add(BuildStringCriterion(propertyPath, value));
                    continue;
                }

                if (TryConvert(value, propertyType, out var typedValue))
                    group.Add(new FilterCriterion(propertyPath, FilterOperator.Equal, typedValue));
                else
                    failures.Add(new ValidationFailure(key, $"The value '{value}' is not valid for the field '{field}'."));
            }

            if (group.Count > 0)
                groups.Add(group);
        }

        return groups;
    }

    private static FilterCriterion BuildStringCriterion(string propertyPath, string value)
    {
        var startsWithWildcard = value.StartsWith(Wildcard);
        var endsWithWildcard = value.EndsWith(Wildcard);
        var term = value.Trim(Wildcard);

        var filterOperator = (startsWithWildcard, endsWithWildcard) switch
        {
            (true, true) => FilterOperator.Contains,
            (false, true) => FilterOperator.StartsWith,
            (true, false) => FilterOperator.EndsWith,
            _ => FilterOperator.Equal
        };

        return new FilterCriterion(propertyPath, filterOperator, term);
    }

    private static Type ResolvePropertyType(Type type, string propertyPath)
    {
        var current = type;
        foreach (var name in propertyPath.Split('.'))
        {
            var property = current.GetProperty(name, BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException($"Property '{name}' not found in '{current.Name}'.");
            current = property.PropertyType;
        }

        return current;
    }

    private static bool IsRangeType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying == typeof(int) || underlying == typeof(long) || underlying == typeof(decimal)
            || underlying == typeof(double) || underlying == typeof(float) || underlying == typeof(DateTime);
    }

    private static bool TryConvert(string value, Type type, out object? result)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        result = null;

        if (underlying == typeof(int) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
            result = i;
        else if (underlying == typeof(long) && long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
            result = l;
        else if (underlying == typeof(decimal) && decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var m))
            result = m;
        else if (underlying == typeof(double) && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            result = d;
        else if (underlying == typeof(float) && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
            result = f;
        else if (underlying == typeof(bool) && bool.TryParse(value, out var b))
            result = b;
        else if (underlying == typeof(Guid) && Guid.TryParse(value, out var g))
            result = g;
        else if (underlying == typeof(DateTime) && DateTime.TryParse(value, CultureInfo.InvariantCulture,
                     DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
            result = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        else if (underlying.IsEnum && Enum.TryParse(underlying, value, ignoreCase: true, out var e) && Enum.IsDefined(underlying, e!))
            result = e;

        return result is not null;
    }
}
