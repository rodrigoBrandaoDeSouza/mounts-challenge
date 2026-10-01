namespace Ambev.DeveloperEvaluation.Domain.Common;

/// <summary>
/// A page of items plus the pagination metadata.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalItems { get; }
    public int CurrentPage { get; }
    public int PageSize { get; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public PagedResult(IReadOnlyList<T> items, int totalItems, int currentPage, int pageSize)
    {
        Items = items;
        TotalItems = totalItems;
        CurrentPage = currentPage;
        PageSize = pageSize;
    }

    /// <summary>Projects the items keeping the pagination metadata.</summary>
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) =>
        new(Items.Select(selector).ToList(), TotalItems, CurrentPage, PageSize);
}
