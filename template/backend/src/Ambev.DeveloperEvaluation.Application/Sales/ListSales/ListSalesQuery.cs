using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Lists the sales with pagination, ordering and filters.
/// </summary>
public class ListSalesQuery : ListQuery, IRequest<PagedResult<SaleResult>>
{
}
