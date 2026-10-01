using MediatR;
using System.Text.Json.Serialization;

namespace Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;

public class UpdateProductCommand : ProductInput, IRequest<ProductResult>
{
    /// <summary>Identifier of the product (taken from the route).</summary>
    [JsonIgnore]
    public int Id { get; set; }
}
