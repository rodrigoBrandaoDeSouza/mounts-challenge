using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

public class ListCartsQuery : ListQuery, IRequest<PagedResult<CartResult>>
{
}
