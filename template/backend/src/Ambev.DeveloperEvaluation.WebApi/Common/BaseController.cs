using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    private static readonly HashSet<string> ReservedQueryParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        "_page", "_size", "_order"
    };

    /// <summary>
    /// Builds a list query from the query string: <c>_page</c>, <c>_size</c>, <c>_order</c>
    /// and every other parameter as a filter.
    /// </summary>
    protected TQuery BuildListQuery<TQuery>(int? page, int? size, string? order) where TQuery : ListQuery, new()
    {
        var query = new TQuery
        {
            Page = page,
            Size = size,
            Order = order
        };

        foreach (var (key, values) in Request.Query)
        {
            if (ReservedQueryParameters.Contains(key))
                continue;

            query.Filters[key] = values.Where(v => v is not null).Select(v => v!).ToArray();
        }

        return query;
    }

    protected IActionResult OkPaginated<T>(PagedResult<T> page) =>
        Ok(PaginatedResponse<T>.From(page));
}
