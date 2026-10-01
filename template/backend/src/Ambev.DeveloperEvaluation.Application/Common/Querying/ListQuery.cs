namespace Ambev.DeveloperEvaluation.Application.Common.Querying;

/// <summary>
/// Raw list parameters, as received from the query string
/// (<c>_page</c>, <c>_size</c>, <c>_order</c> and <c>field=value</c> filters).
/// </summary>
public abstract class ListQuery
{
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? Order { get; set; }

    /// <summary>Filters by field name (as in the JSON response). A field informed more than once is combined with OR.</summary>
    public IDictionary<string, string[]> Filters { get; set; } = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
}
