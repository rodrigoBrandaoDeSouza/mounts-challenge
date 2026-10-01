using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Application.Common.Querying;

/// <summary>
/// Maps the field names exposed by the API (JSON names) to entity property paths,
/// defining which fields can be used to filter and order a resource.
/// </summary>
public sealed class QueryFieldMap<TEntity>
{
    private readonly Dictionary<string, string> _fields = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Ordering applied when the request does not inform <c>_order</c>.</summary>
    public IReadOnlyList<OrderCriterion> DefaultOrdering { get; }

    public QueryFieldMap(IEnumerable<KeyValuePair<string, string>> fields, params OrderCriterion[] defaultOrdering)
    {
        foreach (var field in fields)
            _fields[field.Key] = field.Value;

        DefaultOrdering = defaultOrdering;
    }

    public IEnumerable<string> FieldNames => _fields.Keys;

    public bool TryGetPropertyPath(string field, out string propertyPath)
    {
        if (_fields.TryGetValue(field, out var path))
        {
            propertyPath = path;
            return true;
        }

        propertyPath = string.Empty;
        return false;
    }
}
