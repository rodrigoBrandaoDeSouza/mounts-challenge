using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProducts;

/// <summary>
/// Lists the products with pagination, ordering and filters
/// (also used by <c>GET /products/category/{category}</c>, with the category filter).
/// </summary>
public class ListProductsQuery : ListQuery, IRequest<PagedResult<ProductResult>>
{
}
