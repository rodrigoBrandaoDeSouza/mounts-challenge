namespace Ambev.DeveloperEvaluation.Domain.Common;

/// <summary>
/// Supported filter operators (see General API definitions: equality, wildcards and _min/_max ranges).
/// </summary>
public enum FilterOperator
{
    Equal,
    StartsWith,
    EndsWith,
    Contains,
    GreaterThanOrEqual,
    LessThanOrEqual
}

/// <summary>
/// A single filter over an entity property path (e.g. "Rating.Rate").
/// </summary>
public sealed record FilterCriterion(string PropertyPath, FilterOperator Operator, object? Value);

/// <summary>
/// A single ordering over an entity property path.
/// </summary>
public sealed record OrderCriterion(string PropertyPath, bool Descending);

/// <summary>
/// Pagination, ordering and filtering options for list queries.
/// </summary>
/// <remarks>
/// Filter groups are combined with AND; the criteria inside a group (same field informed
/// more than once) are combined with OR.
/// </remarks>
public sealed class QueryOptions
{
    public const int DefaultPage = 1;
    public const int DefaultSize = 10;
    public const int MaxSize = 100;

    public int Page { get; init; } = DefaultPage;
    public int Size { get; init; } = DefaultSize;
    public IReadOnlyList<OrderCriterion> Orderings { get; init; } = Array.Empty<OrderCriterion>();
    public IReadOnlyList<IReadOnlyList<FilterCriterion>> FilterGroups { get; init; } = Array.Empty<IReadOnlyList<FilterCriterion>>();
}
