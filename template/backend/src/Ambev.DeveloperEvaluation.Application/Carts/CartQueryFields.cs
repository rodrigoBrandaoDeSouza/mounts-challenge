using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Carts;

/// <summary>
/// Fields of the cart resource that can be used in <c>_order</c> and filters.
/// </summary>
public static class CartQueryFields
{
    public static readonly QueryFieldMap<Cart> Map = new(
        new Dictionary<string, string>
        {
            ["id"] = nameof(Cart.Id),
            ["userId"] = nameof(Cart.UserId),
            ["date"] = nameof(Cart.Date)
        },
        new OrderCriterion(nameof(Cart.Id), Descending: false));
}
