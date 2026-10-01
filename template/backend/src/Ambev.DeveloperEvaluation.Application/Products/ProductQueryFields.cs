using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Products;

/// <summary>
/// Fields of the product resource that can be used in <c>_order</c> and filters.
/// </summary>
public static class ProductQueryFields
{
    public static readonly QueryFieldMap<Product> Map = new(
        new Dictionary<string, string>
        {
            ["id"] = nameof(Product.Id),
            ["title"] = nameof(Product.Title),
            ["price"] = nameof(Product.Price),
            ["description"] = nameof(Product.Description),
            ["category"] = nameof(Product.Category),
            ["image"] = nameof(Product.Image),
            ["rating.rate"] = "Rating.Rate",
            ["rating.count"] = "Rating.Count"
        },
        new OrderCriterion(nameof(Product.Id), Descending: false));
}
