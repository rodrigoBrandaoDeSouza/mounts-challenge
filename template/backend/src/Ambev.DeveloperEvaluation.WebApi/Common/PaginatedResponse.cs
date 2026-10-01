using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Response of the list endpoints.
/// </summary>
public sealed class PaginatedResponse<T>
{
    public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();
    public int TotalItems { get; init; }
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }

    public static PaginatedResponse<T> From(PagedResult<T> page) => new()
    {
        Data = page.Items,
        TotalItems = page.TotalItems,
        CurrentPage = page.CurrentPage,
        TotalPages = page.TotalPages
    };
}
